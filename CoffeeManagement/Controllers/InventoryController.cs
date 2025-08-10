using CoffeeManagement.Models;
using CoffeeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeManagement.Controllers
{
    [Authorize]
    public class InventoryController : Controller
    {
        private readonly Gol82750Ecoffee1Context _context;

        public InventoryController(Gol82750Ecoffee1Context context)
        {
            _context = context;
        }

        public IActionResult Index()
        {

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> _GetList(int start = 0, int length = 25, string keyword = "")
        {
            var draw = Request.Form["draw"].ToString();
            var query = _context.Inventories
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(e => e.Name.Contains(keyword) || e.Code.Contains(keyword));
            }

            var totalRecords = await query.CountAsync();

            var list = await query
                .Skip(start)
                .Take(length)
                .Select(x => new InventoryViewModel
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    Quantity = x.Quantity,
                    QuantityStr = x.Quantity.ToString("#,0.0"),
                    Unit = x.Unit,
                    CreatedAt = x.CreatedAt,

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

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Inventory model)
        {
            try
            {
                model.CreatedAt = DateTime.Now;
                _context.Inventories.Add(model);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Tạo thành công", redirect = Url.Action("Index") });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message, detailError = ex.ToString() });
            }

        }

        public async Task<IActionResult> Edit(long id)
        {
            var model = await _context.Inventories.FindAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(long id, Inventory model)
        {
            try
            {
                var exist = await _context.Inventories.FindAsync(id);
                if (exist == null)
                {
                    return NotFound();
                }

                exist.Unit = model.Unit;
                exist.Quantity = model.Quantity;
                exist.Name = model.Name;
                exist.Code = model.Code;
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Lưu thành công", redirect = Url.Action("Index") });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message, detailError = ex.ToString() });
            }

        }

        [HttpPost]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var exist = await _context.Inventories.FindAsync(id);
                if (exist == null)
                {
                    return NotFound();
                }
                _context.Inventories.Remove(exist);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Xóa thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message, detailError = ex.ToString() });
            }


        }
    }
}
