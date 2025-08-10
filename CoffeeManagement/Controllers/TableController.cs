using CoffeeManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeManagement.Controllers
{
    public class TableController : Controller
    {
        private readonly Gol82750Ecoffee1Context _context;

        public TableController(Gol82750Ecoffee1Context context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Table table)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if(await _context.Tables.AnyAsync(t => t.TableNo == table.TableNo))
                    {
                        return Json(new { success = false, message = "Table number already exists." });
                    }
                    _context.Add(table);
                    await _context.SaveChangesAsync();
                    var cardStatusName = table.Status switch
                    {
                        "Available" => "TRỐNG",
                        "Occupied" => "ĐANG DÙNG",
                        "Reserved" => "ĐÃ ĐẶT TRƯỚC",
                        _ => "KHÔNG XÁC ĐỊNH"
                    };
                    return Json(new
                    {
                        success = true,
                        id = table.Id,
                        tableNo = table.TableNo,
                        status = cardStatusName,
                        capacity = table.Capacity,
                        description = table.Description,
                        isActive = table.IsActive
                    });
                }
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "Validation failed: " + string.Join("; ", errors) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }


        [HttpPost]
        public async Task<IActionResult> Transfer(long currentTableId, long newTableId)
        {
            try
            {
                var currentTable = await _context.Tables.FindAsync(currentTableId);
                var newTable = await _context.Tables.FindAsync(newTableId);

                if (currentTable == null || newTable == null)
                {
                    return Json(new { success = false, message = "Table not found." });
                }


                // Chuyển tất cả đơn hàng sang bàn mới
                var orders = await _context.Orders
                    .Where(o => o.TableId == currentTableId && o.IsUsing == true)
                    .ToListAsync();

                foreach (var order in orders)
                {
                    order.TableId = newTableId;
                    _context.Update(order);
                }

                // Cập nhật trạng thái bàn
                currentTable.Status = "Available";
                newTable.Status = "Occupied";
                _context.Update(currentTable);
                _context.Update(newTable);

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    oldTableNo = currentTable.TableNo,
                    newTableNo = newTable.TableNo
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var table = await _context.Tables.FindAsync(id);
                if (table == null)
                {
                    return Json(new { success = false, message = "Table not found." });
                }

                // Kiểm tra xem bàn có đơn hàng đang sử dụng không
                var hasActiveOrders = await _context.Orders.AnyAsync(o => o.TableId == id && o.IsUsing == true);
                if (hasActiveOrders)
                {
                    return Json(new { success = false, message = "Cannot delete table with active orders." });
                }

                _context.Tables.Remove(table);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }



    }
}
