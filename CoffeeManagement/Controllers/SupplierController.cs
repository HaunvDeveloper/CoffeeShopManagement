using CoffeeManagement.Models;
using CoffeeManagement.Services;
using CoffeeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeManagement.Controllers
{
    [Authorize]
    public class SupplierController : Controller
    {
        private readonly Gol82750Ecoffee1Context _context;
        private readonly SupplierService _supplierService;

        public SupplierController(Gol82750Ecoffee1Context context)
        {
            _context = context;
            _supplierService = new SupplierService(context);
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> _GetList(int start = 0, int length = 25, string keyword = "")
        {
            var draw = Request.Form["draw"].ToString();
            var query = _context.Suppliers
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(e => (e.Name != null && e.Name.Contains(keyword)) || (e.ShortName != null && e.ShortName.Contains(keyword)));
            }

            var totalRecords = await query.CountAsync();

            var list = await query
                .Skip(start)
                .Take(length)
                .Select(x => new SupplierViewModel
                {
                    Id = x.Id,
                    SupplierNo = x.SupplierNo,
                    ShortName = x.ShortName,
                    Name = x.Name,
                    PhoneNo = x.PhoneNo,
                    Email = x.Email,
                    Address = x.Address,
                })
                .ToListAsync();

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
            ViewBag.SupplierNo = _supplierService.GenerateNewSupplierNo();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Supplier model)
        {
            try
            {
                model.CreatedAt = DateTime.Now;
                _context.Suppliers.Add(model);
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
            var model = await _context.Suppliers.FindAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(long id, Supplier model)
        { 
            try
            {
                var exist = await _context.Suppliers.FindAsync(id);
                if (exist == null)
                {
                    return NotFound();
                }

                exist.SupplierNo = model.SupplierNo;
                exist.ShortName = model.ShortName;
                exist.Address = model.Address;
                exist.Name = model.Name;
                exist.Email = model.Email;
                exist.PhoneNo = model.PhoneNo;
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Lưu thành công", redirect = Url.Action("Index") });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message, detailError = ex.ToString() });
            }

        }

    }
}
