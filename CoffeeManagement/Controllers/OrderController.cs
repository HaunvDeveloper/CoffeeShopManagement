using CoffeeManagement.Models;
using CoffeeManagement.Services;
using CoffeeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NuGet.Protocol;
using System.Security.Claims;
using System.Text;

namespace CoffeeManagement.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly Gol82750Ecoffee1Context _context;
        private readonly OrderService _saleService;

        public OrderController(Gol82750Ecoffee1Context context)
        {
            _context = context;
            _saleService = new OrderService(context);
        }

        public IActionResult Index()
        {

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> _GetList(int start = 0, int length = 25, string keyword = "")
        {
            var draw = Request.Form["draw"].ToString();
            var query = _context.Orders
                .Include(x => x.EmployeeSale)
                .Include(x => x.Table)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(e => e.SaleNo.Contains(keyword) || e.CustomerName.Contains(keyword));
            }

            var totalRecords = await query.CountAsync();
            var dictionary = new Dictionary<string, string>
            {
                { "cash", "Tiền mặt" },
                { "card", "Thẻ" },
                { "mobile", "Chuyển khoản" },
                { "", "Chưa thanh toán" } // Default case for null or empty payment method}
            };
            var list = await query
                .OrderByDescending(x => x.SaleDate)
                .Skip(start)
                .Take(length)
                .Select(x => new 
                {
                    id = x.Id,
                    saleNo = x.SaleNo,
                    billNo = x.BillNo,
                    customerId = x.CustomerId,
                    customerName = x.CustomerName,
                    saleDate = x.SaleDate.ToString("HH:mm dd/MM/yyyy"),
                    paymentDate = x.PaymentDate.Value.ToString("HH:mm dd/MM/yyyy"),
                    employeeId = x.EmployeeSaleId,
                    employeeName = x.EmployeeSale != null ? x.EmployeeSale.FullName : "",
                    totalAmount = x.TotalAmount,
                    createdUserId = x.CreatedUserId,
                    status = x.IsPaid ? "<div class='badge bg-success'>ĐÃ THANH TOÁN</div>" : "<div class='badge bg-danger'>CHƯA THANH TOÁN</div>",
                    paymentMethod = x.PaymentMethod != null ? dictionary[x.PaymentMethod] : "Chưa xác định",
                    position = x.IsTakeHome ? "Mang về" : x.TableId != null ?  $"Bàn {x.Table.TableNo}" : "<div>Chưa có bàn</div>",
                    isUsing = x.IsUsing == true ? "Đang sử dụng" : "Đã rời bàn",
                    description = x.Description,

                })
                .ToListAsync();

            // Trả về dữ liệu theo định dạng JSON để DataTable sử dụng
            return Json(new
            {
                draw = draw, // DataTable yêu cầu parameter 'draw' để đồng bộ các yêu cầu
                recordsTotal = totalRecords,  // Tổng số bản ghi trong cơ sở dữ liệu
                recordsFiltered = totalRecords, // Số bản ghi thỏa mãn điều kiện tìm kiếm
                data = list // Dữ liệu trả về
            });
        }


        public IActionResult ChooseTable()
        {
            var tables = _context.Tables
                .Include(x => x.Orders)
                    .ThenInclude(o => o.OrderDetails)
                .AsNoTracking()
                .OrderBy(x => x.TableNo)
                .Select(x => new TableViewModel
                {
                    Id = x.Id,
                    TableNo = x.TableNo,
                    Status = x.Status,
                    IsActive = x.IsActive,
                    Capacity = x.Capacity,
                    Description = x.Description,
                    IsPaid = x.Orders
                        .Where(o => o.TableId == x.Id && o.IsUsing == true)
                        .All(o => o.IsPaid == true),
                    FirstGetIn = x.Orders
                        .Where(o => o.TableId == x.Id && o.IsUsing == true)
                        .OrderBy(o => o.SaleDate)
                        .Select(o => (DateTime?)o.SaleDate)
                        .FirstOrDefault(),
                    TotalAmount = x.Orders
                        .Where(o => o.TableId == x.Id && o.IsUsing == true)
                        .Sum(o => o.TotalAmount),
                    RemainingAmount = x.Orders
                        .Where(o => o.TableId == x.Id && o.IsUsing == true && o.IsPaid == false)
                        .Sum(o => o.TotalAmount),
                    TotalItem = (int)x.Orders
                        .Where(o => o.TableId == x.Id && o.IsUsing == true)
                        .SelectMany(o => o.OrderDetails)
                        .Sum(od => od.Quantity)
                })
                .ToList();

            return View(tables);
        }

        public IActionResult CheckingOrder(long tableId)
        {
            var table = _context.Tables.Find(tableId);
            if (table == null || !table.IsActive)
            {
                return NotFound();
            }

            var orders = _context.Orders
                .Include(x => x.OrderDetails)
                .Where(o => o.TableId == tableId && o.IsUsing == true)
                .ToList();

            ViewBag.TableId = tableId;
            ViewBag.TableNo = table.TableNo;
            return View(orders);
        }

        public IActionResult Order(long tableId)
        {
            var products = _context.Products
                .Where(p => p.IsActive)
                .ToList();
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            ViewBag.Customers = _context.Customers
                .AsNoTracking()
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.CustomerNo + " - " +  x.LastName + " " + x.FirstName,
                })
                .ToList();
            ViewBag.Tables = _context.Tables
                .AsNoTracking()
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = "Bàn " + x.TableNo,
                    Selected = x.Id == tableId
                })
                .ToList();
            return View(products);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(long saleId, OrderRequest order)
        {
            try
            {
                if (order.Items == null || order.Items.Count == 0)
                {
                    return Json(new { success = false, message = "No items in order." });
                }
                long userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var employee = _context.Employees
                    .AsNoTracking()
                    .FirstOrDefault(x => x.UserId == userId);

                var newOrder = new Order
                {
                    CreatedUserId = userId,
                    EmployeeSaleId = employee?.Id,
                    SaleDate = DateTime.Now,
                    PaymentMethod = order.PaymentMethod,
                    TotalAmount = order.Items.Sum(x => x.TotalPrice),
                    CustomerId = order.CustomerId == 0 ? null : order.CustomerId,
                    CustomerName = order.CustomerId == 0 ? "Khách vãng lai" : 
                        _context.Customers.AsNoTracking()
                            .Where(c => c.Id == order.CustomerId)
                            .Select(c => c.LastName + " " + c.FirstName)
                            .FirstOrDefault(),
                    IsPaid = order.PaymentMethod != null,
                    IsUsing = order.IsTakeHome ? false : true, 
                    PaymentDate = order.PaymentMethod != null ? DateTime.Now : null,
                    TableId = (order.IsTakeHome || order.TableId == 0 ? null : order.TableId),
                    SaleStatusId = (long)SaleStatusEnum.Pending,
                    IsTakeHome = order.IsTakeHome,
                    BillNo = _saleService.GenerateNewBillNo(),
                    SaleNo = _saleService.GenerateNewSaleNo(),
                    OrderDetails = order.Items.Select(item => new OrderDetail
                    {
                        ProductId = item.Id,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price,
                        Discount = item.Discount
                    }).ToList()
                };
                
                _context.Orders.Add(newOrder);

                if (newOrder.TableId != 0)
                {
                    var table = await _context.Tables.FindAsync(order.TableId);
                    if (table != null)
                    {
                        table.Status = "Occupied";
                        // Thông báo SignalR (nếu có)
                        //await _hubContext.Clients.All.SendAsync("UpdateTableStatus", table.Id, "Occupied");
                    }
                }


                await _context.SaveChangesAsync();

                return Json(new { success = true, message="Thành công", isPaid = newOrder.IsPaid, redirect=Url.Action("OrderConfirm", "Order", new {id=newOrder.Id}) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        
        [HttpPost]
        public IActionResult EditOrder(long saleId, OrderRequest order)
        {
            try
            {
                if (order.Items == null || order.Items.Count == 0)
                {
                    return Json(new { success = false, message = "No items in order." });
                }

                var sale = _context.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefault(o => o.Id == saleId);

                if (sale == null)
                {
                    return Json(new { success = false, message = "Order not found." });
                }

                // Cập nhật trạng thái bàn cũ (nếu có)
                if (sale.TableId != 0 && sale.TableId != order.TableId)
                {
                    var oldTable = _context.Tables.Find(sale.TableId);
                    if (oldTable != null)
                    {
                        oldTable.Status = "Available"; // Đặt lại trạng thái bàn cũ
                        _context.Update(oldTable);
                    }
                }

                // Cập nhật thông tin đơn hàng
                sale.PaymentMethod = order.PaymentMethod;
                sale.TotalAmount = order.Items.Sum(x => x.TotalPrice);
                sale.CustomerId = order.CustomerId == 0 ? null : order.CustomerId;
                sale.CustomerName = order.CustomerId == 0 ? "Khách vãng lai" :
                    _context.Customers.AsNoTracking()
                        .Where(c => c.Id == order.CustomerId)
                        .Select(c => c.LastName + " " + c.FirstName)
                        .FirstOrDefault();
                sale.IsPaid = order.PaymentMethod != null;
                sale.TableId = (order.IsTakeHome ? null : order.TableId);
                sale.PaymentDate = order.PaymentMethod != null ? DateTime.Now : null;
                sale.IsTakeHome = order.IsTakeHome;
                sale.IsUsing = sale.IsTakeHome ? false : sale.IsUsing; 

                // Cập nhật trạng thái bàn mới (nếu có)
                if (order.TableId != 0)
                {
                    var newTable = _context.Tables.Find(order.TableId);
                    if (newTable != null)
                    {
                        newTable.Status = "Occupied";
                        _context.Update(newTable);
                    }
                }

                // Xóa OrderDetails cũ
                _context.OrderDetails.RemoveRange(sale.OrderDetails);

                // Thêm OrderDetails mới
                sale.OrderDetails = order.Items.Select(item => new OrderDetail
                {
                    ProductId = item.Id,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price,
                    Discount = item.Discount
                }).ToList();

                _context.Update(sale);
                _context.SaveChanges();

                return Json(new { success = true, message = "Thành công", isPaid = sale.IsPaid, redirect = Url.Action("OrderConfirm", "Order", new { id = sale.Id }) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }


        [HttpGet]
        public IActionResult GetOrder(long saleId)
        {
            try
            {
                var order = _context.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                    .Select(o => new
                    {
                        o.Id,
                        o.CustomerId,
                        o.TableId,
                        o.PaymentMethod,
                        orderDetails = o.OrderDetails.Select(od => new
                        {
                            productId = od.ProductId,
                            productName = od.Product.Name,
                            unitPrice = od.UnitPrice,
                            quantity = od.Quantity,
                            discount = od.Discount // Giả định không có discount trong OrderDetail, nếu có thì thêm
                        }).ToList()
                    })
                    .FirstOrDefault(o => o.Id == saleId);

                if (order == null)
                {
                    return Json(new { success = false, message = "Order not found." });
                }

                return Json(new { success = true, order });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> LeaveTable(long tableId)
        {
            try
            {
                var table = await _context.Tables.FindAsync(tableId);
                if (table == null)
                {
                    return Json(new { success = false, message = "Table not found." });
                }
                // Cập nhật trạng thái bàn
                table.Status = "Available";
                _context.Tables.Update(table);
                // Cập nhật tất cả các đơn hàng đang sử dụng bàn này
                var orders = _context.Orders.Where(o => o.TableId == tableId && o.IsUsing == true).ToList();
                foreach (var order in orders)
                {
                    order.IsUsing = false;
                    order.LeaveTime = DateTime.Now; // Ghi lại thời gian rời bàn
                    _context.Orders.Update(order);
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Bàn đã được dọn sạch." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> LeaveOrder(long orderId)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    return Json(new { success = false, message = "Order not found." });
                }
                bool isRedirect = false;
                // Cập nhật trạng thái bàn nếu có
                if (order.TableId != 0)
                {
                    var existingOrder = _context.Orders
                        .Where(o => o.TableId == order.TableId && o.IsUsing == true)
                        .Count();
                    if (existingOrder == 1) // Chỉ có đơn hàng này đang sử dụng bàn
                    {
                        var table = await _context.Tables.FindAsync(order.TableId);
                        if (table != null)
                        {
                            table.Status = "Available";
                            _context.Tables.Update(table);
                            isRedirect = true; // Đặt cờ để chuyển hướng về trang chọn bàn
                        }
                    }
                }
                order.IsUsing = false;
                order.LeaveTime = DateTime.Now; // Ghi lại thời gian rời bàn
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Đơn hàng đã xác nhận thời gian rời đi.", isRedirect, redirect = Url.Action("ChooseTable") });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> QuickPaymentByOrder(long orderId, string paymentMethod)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    return Json(new { success = false, message = "Order not found." });
                }
                // Cập nhật thông tin thanh toán
                order.IsPaid = true;
                order.PaymentMethod = paymentMethod;
                order.PaymentDate = DateTime.Now;

                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Thanh toán thành công.", redirect = Url.Action("OrderConfirm", "Order", new { id = order.Id }) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> QuickPaymentByTable(long tableId, string paymentMethod)
        {
            try
            {
                var orders = _context.Orders
                    .Where(o => o.TableId == tableId && o.IsUsing == true)
                    .ToList();
                if (orders.Count == 0)
                {
                    return Json(new { success = false, message = "No active orders found for this table." });
                }
                foreach (var order in orders)
                {
                    order.IsPaid = true;
                    order.PaymentMethod = paymentMethod;
                    order.PaymentDate = DateTime.Now;
                    _context.Orders.Update(order);
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Thanh toán thành công.", redirect = Url.Action("OrderConfirm", "Order", new { id = orders.First().Id }) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        public IActionResult Edit(long id)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }
            // Lấy thông tin sản phẩm cho OrderDetails
            ViewBag.Products = _context.Products
                .Where(p => p.IsActive)
                .ToList();
            // Lấy danh sách bàn
            ViewBag.Tables = _context.Tables
                .AsNoTracking()
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = "Bàn " + x.TableNo,
                    Selected = x.Id == order.TableId
                })
                .ToList();
            // Lấy danh sách khách hàng
            ViewBag.Customers = _context.Customers
                .AsNoTracking()
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.CustomerNo + " - " + x.LastName + " " + x.FirstName,
                    Selected = x.Id == order.CustomerId
                })
                .ToList();
            return View(order);
        }


        [HttpPost]
        [Authorize(Roles = "admin,manager")]
        public IActionResult Edit(Order order)
        {
            try
            {
                if (order.OrderDetails == null || order.OrderDetails.Count == 0)
                {
                    return Json(new { success = false, message = "No items in order." });
                }

                var oldOrder = _context.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefault(o => o.Id == order.Id);

                if (oldOrder == null)
                {
                    return Json(new { success = false, message = "Order not found." });
                }

                // Cập nhật trạng thái bàn cũ (nếu có)
                if (oldOrder.IsUsing == true &&  oldOrder.TableId != 0 && oldOrder.TableId != order.TableId)
                {
                    var oldTable = _context.Tables.Find(oldOrder.TableId);
                    if (oldTable != null)
                    {
                        oldTable.Status = "Available"; // Đặt lại trạng thái bàn cũ
                        _context.Update(oldTable);
                    }
                }

                // Cập nhật thông tin đơn hàng
                oldOrder.PaymentMethod = string.IsNullOrEmpty(order.PaymentMethod) ? null : order.PaymentMethod;
                oldOrder.TotalAmount = order.OrderDetails.Sum(x => x.UnitPrice * x.Quantity * (1 - x.Discount / 100));
                oldOrder.CustomerId = order.CustomerId == 0 ? null : order.CustomerId;
                oldOrder.CustomerName = order.CustomerId == 0 || order.CustomerId == null ? "Khách vãng lai" :
                    _context.Customers.AsNoTracking()
                        .Where(c => c.Id == order.CustomerId)
                        .Select(c => c.LastName + " " + c.FirstName)
                        .FirstOrDefault();
                oldOrder.IsPaid = !string.IsNullOrEmpty(order.PaymentMethod);
                oldOrder.TableId = (order.IsTakeHome ? null : order.TableId);
                oldOrder.PaymentDate = !string.IsNullOrEmpty(order.PaymentMethod) ? order.PaymentDate ?? DateTime.Now  : null;
                oldOrder.IsTakeHome = order.IsTakeHome;
                oldOrder.IsUsing = oldOrder.IsTakeHome ? false : oldOrder.IsUsing;
                oldOrder.BillNo = order.BillNo;
                oldOrder.SaleDate = order.SaleDate;
                oldOrder.Description = order.Description;
                oldOrder.PaymentDate = order.PaymentMethod != null ? DateTime.Now : null;

                

                // Xóa OrderDetails cũ
                _context.OrderDetails.RemoveRange(oldOrder.OrderDetails);

                // Thêm OrderDetails mới
                oldOrder.OrderDetails = order.OrderDetails.ToList();

                _context.Update(oldOrder);
                _context.SaveChanges();

                return Json(new { success = true, message = "Thành công", isPaid = oldOrder.IsPaid, redirect = Url.Action("Index", "Order", new { id = oldOrder.Id }) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "admin,manager")]
        public IActionResult Delete(long id)
        {
            try
            {
                var order = _context.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefault(o => o.Id == id);
                if (order == null)
                {
                    return Json(new { success = false, message = "Order not found." });
                }
                // Xóa OrderDetails liên quan
                _context.OrderDetails.RemoveRange(order.OrderDetails);
                // Xóa Order
                _context.Orders.Remove(order);
                _context.SaveChanges();
                return Json(new { success = true, message = "Order deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        public IActionResult OrderConfirm(long id)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            // Lấy thông tin sản phẩm cho OrderDetails
            ViewBag.Products = _context.Products
                .Where(p => p.IsActive)
                .ToList();

            // Lấy TableNo nếu không phải Bán Mang Đi
            if (order.TableId != 0)
            {
                ViewBag.TableNo = _context.Tables
                    .Where(t => t.Id == order.TableId)
                    .Select(t => t.TableNo)
                    .FirstOrDefault();
            }

            return View(order);
        }


        [HttpGet]
        public async Task<IActionResult> ExportInvoice(long orderId)
        {
            // Retrieve the order with its details
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound("Order not found.");
            }

            // Generate invoice content (HTML for printing)
            var invoiceHtml = GenerateInvoiceHtml(order);

            // Save export record to ExportBill
            var exportBill = new ExportBill
            {
                OrderId = orderId,
                ExportDate = DateTime.Now,
                Status = "Success" // Can be updated based on print result
            };

            _context.ExportBills.Add(exportBill);
            await _context.SaveChangesAsync();

            // Return view with invoice content for printing
            ViewBag.InvoiceContent = invoiceHtml;
            ViewBag.OrderId = orderId;
            ViewBag.ExportBillId = exportBill.Id; // For tracking

            return View("ExportInvoice"); // Use a separate view for printing
        }


        private string GenerateInvoiceHtml(Order order)
        {
            var products = _context.Products.Where(p => p.IsActive).ToList();
            var tableNo = order.TableId == 0 ? "Bán Mang Đi" : _context.Tables
                .Where(t => t.Id == order.TableId)
                .Select(t => t.TableNo)
                .FirstOrDefault();

            decimal total = 0;
            var rowsBuilder = new StringBuilder();

            foreach (var detail in order.OrderDetails)
            {
                var product = products.FirstOrDefault(p => p.Id == detail.ProductId);
                var itemTotal = detail.Quantity * detail.UnitPrice;
                total += itemTotal;

                rowsBuilder.Append($@"
            <tr>
                <td>{product?.Name ?? "Unknown Product"}</td>
                <td>{detail.Quantity}</td>
                <td>{detail.UnitPrice.ToString("N0")} VND</td>
                <td>{itemTotal.ToString("N0")} VND</td>
            </tr>");
            }
            var dictionary = new Dictionary<string, string>
            {
                { "cash", "Tiền mặt" },
                { "card", "Thẻ" },
                { "mobile", "Chuyển khoản" },
                { "", "Chưa thanh toán" } // Default case for null or empty payment method}
            };
            return $@"
    <!DOCTYPE html>
    <html>
    <head>
        <title>Hóa Đơn Bán Hàng</title>
        <style>
            body {{ margin: 0; padding: 0; }}
                    @media print {{
                        body {{
                            width: 80mm; /* Adjust to 57mm if needed */
                            margin: 0 auto;
                            font-family: Arial, sans-serif;
                            font-size: 12px;
                        }}
                        .invoice {{
                            width: 80mm; /* Match body width */
                            margin: 0;
                            padding: 5mm;
                        }}
                        .header {{ text-align: center; border-bottom: 1px dashed #000; padding-bottom: 2mm; }}
                        .header h2 {{ margin: 0; font-size: 14px; }}
                        .details {{ margin-top: 2mm; }}
                        .details p {{ margin: 1mm 0; font-size: 10px; }}
                        .table {{ width: 100%; border-collapse: collapse; font-size: 10px; }}
                        .table th, .table td {{ border: 1px solid #000; padding: 1mm; text-align: left; }}
                        .table th {{ background-color: #f5f5f5; }}
                        .footer {{ margin-top: 2mm; text-align: center; border-top: 1px dashed #000; padding-top: 2mm; font-size: 10px; }}
                        .no-print {{ display: none; }}
                    }}
                    @page {{
                        size: 80mm auto; /* Match width, height auto */
                        margin: 0;
                    }}
        </style>
    </head>
    <body>
        <div class='invoice'>
            <div class='header'>
                <h2>HÓA ĐƠN BÁN HÀNG</h2>
                <p>Mã đơn hàng: {order.BillNo}</p>
            </div>
            <div class='details'>
                <p><strong>Số đơn hàng:</strong> {order.SaleNo}</p>
                <p><strong>Ngày:</strong> {order.SaleDate:dd/MM/yyyy HH:mm}</p>
                <p><strong>Bàn:</strong> {tableNo}</p>
                <p><strong>Khách hàng:</strong> {order.CustomerName ?? "Khách lẻ"}</p>
                <p><strong>Phương thức thanh toán:</strong> {dictionary[order.PaymentMethod ?? ""]}</p>
            </div>
            <table class='table'>
                <thead>
                    <tr>
                        <th>Sản phẩm</th>
                        <th>Số lượng</th>
                        <th>Đơn giá</th>
                        <th>Thành tiền</th>
                    </tr>
                </thead>
                <tbody>
                    {rowsBuilder}
                </tbody>
                <tfoot>
                    <tr>
                        <th colspan='3' style='text-align:right;'>Tổng cộng:</th>
                        <td>{total.ToString("N0")} VND</td>
                    </tr>
                </tfoot>
            </table>
            <div class='footer'>
                <p>Cảm ơn quý khách!</p>
            </div>
        </div>
        <script>
            window.print();
            window.onafterprint = function () {{
                window.location.href = '/Order/ChooseTable';
            }};
        </script>
    </body>
    </html>";
        }

        [HttpPost]
        public IActionResult GetShortInfor(long id)
        {
            try
            {
                var model = _context.Orders
                    .AsNoTracking()
                    .Include(p => p.Customer)
                    .Include(p => p.EmployeeSale)
                    .FirstOrDefault(p => p.Id == id);

                if (model == null)
                {
                    return Json(new { success = false, error = "Sale ID not found" });
                }

                var viewModel = new SaleViewModel(model);

                return Json(new { success = true, data = viewModel });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        //public IActionResult Create()
        //{
        //    ViewBag.NewSaleNo = _saleService.GenerateNewSaleNo();
        //    ViewBag.Customers = _context.Customers
        //        .AsNoTracking()
        //        .Select(x => new SelectListItem
        //        {
        //            Value = x.Id.ToString(),
        //            Text = x.LastName + " " + x.FirstName,
        //        })
        //        .ToList();
        //    ViewBag.Inventories = _context.Inventories
        //        .AsNoTracking()
        //        .ToList();
        //    return View();
        //}

        //[HttpPost]
        //public async Task<IActionResult> Create(Order model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        //            var employee = _context.Employees.Single(x => x.UserId == userId);
        //            model.CreatedUserId = userId;
        //            model.EmployeeSaleId = employee.Id;
        //            model.TotalAmount = model.SaleDetails.Sum(x => x.Quantity * x.UnitPrice * (1 + x.Vat / 100));
        //            _context.Sales.Add(model);
        //            await _context.SaveChangesAsync();
        //            return Json(new { success = true, message = "Tạo thành công!!!", redirect = Url.Action("Index") });
        //        }
        //        catch (Exception ex)
        //        {
        //            return Json(new
        //            {
        //                success = false,
        //                error = ex.Message,
        //                errorMessage = ex.ToString(),
        //            });
        //        }
        //    }
        //    else
        //    {
        //        var errors = ModelState
        //            .Where(ms => ms.Value?.Errors.Count > 0)
        //            .Select(ms => $"{ms.Key}: {string.Join(", ", ms.Value?.Errors.Select(e => e.ErrorMessage) ?? [])}")
        //            .ToList();

        //        string errorMessage = string.Join(" | ", errors);
        //        return Json(new
        //        {
        //            success = false,
        //            error = "Dữ liệu nhập không hợp lệ!!",
        //            errorMessage = errorMessage,
        //        });
        //    }
        //}



        //[HttpPost]
        //public async Task<IActionResult> Delete(long id)
        //{
        //    try
        //    {
        //        var sale = await _context.Sales.FindAsync(id);
        //        if (sale == null)
        //        {
        //            return Json(new { success = false, message = "Không tìm thấy phiếu bán cần xóa." });
        //        }

        //        _context.Sales.Remove(sale);
        //        await _context.SaveChangesAsync();

        //        return Json(new { success = true, message = "Xóa phiếu bán thành công." });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = "Có lỗi xảy ra khi xóa: " + ex.Message });
        //    }
        //}

        //public async Task<IActionResult> Edit(long id)
        //{
        //    try
        //    {
        //        var model = await _context.Sales
        //            .AsNoTracking()
        //            .Include(p => p.SaleDetails)
        //                .ThenInclude(dt => dt.Product)
        //            .FirstOrDefaultAsync(p => p.Id == id);

        //        if (model == null)
        //        {
        //            return NotFound(new { success = false, error = "Sale ID not found" });
        //        }

        //        ViewBag.Customers = _context.Customers
        //            .AsNoTracking()
        //            .Select(x => new SelectListItem
        //            {
        //                Value = x.Id.ToString(),
        //                Text = x.LastName + " " + x.FirstName,
        //            })
        //            .ToList();
        //        ViewBag.Inventories = _context.Inventories
        //            .AsNoTracking()
        //            .ToList();

        //        return View(model);
        //    }
        //    catch (Exception ex)
        //    {
        //        return NotFound(new { success = false, error = ex.Message });
        //    }
        //}

        //[HttpPost]
        //public async Task<IActionResult> Edit(Order model)
        //{

        //    var existingSale = await _context.Orders
        //        .Include(p => p.OrderDetails)
        //        .FirstOrDefaultAsync(p => p.Id == model.Id);

        //    if (existingSale == null)
        //    {
        //        return Json(new { success = false, error = "Không tìm thấy bản ghi cần cập nhật." });
        //    }

        //    existingSale.CustomerId = model.CustomerId;
        //    existingSale.CustomerName = model.CustomerName;
        //    existingSale.BillNo = model.BillNo;
        //    existingSale.SaleDate = model.SaleDate;
        //    existingSale.PaymentDate = model.PaymentDate;
        //    existingSale.PaymentStatus = model.PaymentStatus;
        //    existingSale.Description = model.Description;
        //    existingSale.TotalAmount = model.OrderDetails.Sum(x => x.Quantity * x.UnitPrice * (1 + x.Vat / 100));


        //    _context.OrderDetails.RemoveRange(existingSale.OrderDetails);

        //    foreach (var item in model.OrderDetails)
        //    {
        //        existingSale.OrderDetails.Add(new OrderDetail
        //        {
        //            ProductId = item.ProductId,
        //            ProductName = item.ProductName,
        //            Quantity = item.Quantity,
        //            UnitPrice = item.UnitPrice,
        //            Vat = item.Vat,
        //            Description = item.Description
        //        });
        //    }

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //        return Json(new { success = true, redirect = Url.Action("Index", "Purchase") });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, error = "Lỗi khi cập nhật dữ liệu: " + ex.Message });
        //    }
        //}
    }
}
