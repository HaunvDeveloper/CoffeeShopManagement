using CoffeeManagement.Models;
using CoffeeManagement.Services;
using CoffeeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using System.Security.Claims;

namespace CoffeeManagement.Controllers
{
    [Authorize]
    public class PurchaseController : Controller
    {
        private readonly Gol82750Ecoffee1Context _context;
        private readonly PurchaseService _purchaseService;

        public PurchaseController(Gol82750Ecoffee1Context context)
        {
            _context = context;
            _purchaseService = new PurchaseService(context);
        }

        public IActionResult Index()
        {

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> _GetList(int start = 0, int length = 25, string keyword = "")
        {
            var draw = Request.Form["draw"].ToString();
            var query = _context.Purchases
                .Include(x => x.EmployeeSale)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(e => e.PurchasedNo.Contains(keyword) || e.SupplierName.Contains(keyword));
            }

            var totalRecords = await query.CountAsync();

            var list = await query
                .Skip(start)
                .Take(length)
                .Select(x => new PurchaseViewModel
                {
                    Id = x.Id,
                    PurchasedNo = x.PurchasedNo,
                    BillNo = x.BillNo,
                    SupplierName = x.SupplierName,
                    SupplierId = x.SupplierId,
                    PurchasedDate = x.PurchasedDate,
                    PurchasedDateStr = x.PurchasedDate.ToString("dd/MM/yyyy"),
                    PaymentDate = x.PaymentDate,
                    PaymentDateStr = x.PaymentDate.ToString("dd/MM/yyyy"),
                    EmployeeSaleId = x.EmployeeSaleId,
                    EmployeeName = x.EmployeeSale.FullName,
                    TotalAmount = x.TotalAmount,
                    PaymentStatus = x.PaymentStatus,
                    CreatedUserId = x.CreatedUserId,
                    Description = x.Description,
                    TotalAmountStr = x.TotalAmount.Value.ToString("#0,0 VND")

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

        public IActionResult Create()
        {
            ViewBag.NewPurchaseNo = _purchaseService.GenerateNewPurchaseNo();
            ViewBag.Suppliers = _context.Suppliers
                .AsNoTracking()
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.ShortName + " - " + x.Name,
                })
                .ToList();
            ViewBag.Inventories = _context.Inventories
                .AsNoTracking()
                .ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Purchase model)
        {

            try
            {
                long userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var employee = _context.Employees.Single(x => x.UserId == userId);
                model.CreatedUserId = userId;
                model.EmployeeSaleId = employee.Id;
                model.TotalAmount = model.PurchaseDetails.Sum(x => x.Quantity * x.UnitPrice * (1 + x.Vat / 100));
                _context.Purchases.Add(model);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Tạo thành công!!!", redirect = Url.Action("Index") });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    errorMessage = ex.ToString(),
                });
            }


        }

        [HttpPost]
        public IActionResult GetShortInfor(long id)
        {
            try
            {
                var model = _context.Purchases
                    .AsNoTracking()
                    .Include(p => p.Supplier)
                    .Include(p => p.EmployeeSale)
                    .FirstOrDefault(p => p.Id == id);

                if (model == null)
                {
                    return Json(new { success = false, error = "Purchase ID not found" });
                }

                var viewModel = new PurchaseViewModel(model);

                return Json(new { success = true, data = viewModel });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var purchase = await _context.Purchases.FindAsync(id);
                if (purchase == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy phiếu nhập cần xóa." });
                }

                _context.Purchases.Remove(purchase);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Xóa phiếu nhập thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa: " + ex.Message });
            }
        }

        public async Task<IActionResult> Edit(long id)
        {
            try
            {
                var model = await _context.Purchases
                    .AsNoTracking()
                    .Include(p => p.PurchaseDetails)
                        .ThenInclude(dt => dt.Item)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (model == null)
                {
                    return NotFound(new { success = false, error = "Purchase ID not found" });
                }

                ViewBag.NewPurchaseNo = _purchaseService.GenerateNewPurchaseNo();
                ViewBag.Suppliers = _context.Suppliers
                    .AsNoTracking()
                    .Select(x => new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = x.ShortName + " - " + x.Name,
                    })
                    .ToList();
                ViewBag.Inventories = _context.Inventories
                    .AsNoTracking()
                    .ToList();

                return View(model);
            }
            catch (Exception ex)
            {
                return NotFound(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Purchase model)
        {

            var existingPurchase = await _context.Purchases
                .Include(p => p.PurchaseDetails)
                .FirstOrDefaultAsync(p => p.Id == model.Id);

            if (existingPurchase == null)
            {
                return Json(new { success = false, error = "Không tìm thấy bản ghi cần cập nhật." });
            }

            existingPurchase.SupplierId = model.SupplierId;
            existingPurchase.SupplierName = model.SupplierName;
            existingPurchase.BillNo = model.BillNo;
            existingPurchase.PurchasedDate = model.PurchasedDate;
            existingPurchase.PaymentDate = model.PaymentDate;
            existingPurchase.PaymentStatus = model.PaymentStatus;
            existingPurchase.Description = model.Description;
            existingPurchase.TotalAmount = model.PurchaseDetails.Sum(x => x.Quantity * x.UnitPrice * (1 + x.Vat / 100));


            _context.PurchaseDetails.RemoveRange(existingPurchase.PurchaseDetails);

            foreach (var item in model.PurchaseDetails)
            {
                existingPurchase.PurchaseDetails.Add(new PurchaseDetail
                {
                    ItemId = item.ItemId,
                    ItemName = item.ItemName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Vat = item.Vat,
                    Description = item.Description
                });
            }

            try
            {
                await _context.SaveChangesAsync();
                return Json(new { success = true, redirect = Url.Action("Index", "Purchase") });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Lỗi khi cập nhật dữ liệu: " + ex.Message });
            }
        }








    }
}
