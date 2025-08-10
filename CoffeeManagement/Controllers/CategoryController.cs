using CoffeeManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeManagement.Controllers
{
    public class CategoryController : Controller
    {
        private readonly Gol82750Ecoffee1Context _context;

        public CategoryController(Gol82750Ecoffee1Context context)
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
            var query = _context.Categories.AsQueryable();
            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(e => e.Name.Contains(keyword) || e.Code.Contains(keyword));
            }
            var totalRecords = await query.CountAsync();
            var list = await query
                .Skip(start)
                .Take(length)
                .Select(x => new
                {
                    x.Id,
                    x.Code,
                    x.Name,
                    x.Description,
                })
                .ToListAsync();
            return Json(new { draw, recordsTotal = totalRecords, recordsFiltered = totalRecords, data = list });
        }

        [HttpPost]
        public async Task<JsonResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                if (_context.Categories.Any(c => c.Code == category.Code))
                {
                    return Json(new { success = false, message = "Category code already exists." });
                }
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Category created successfully.", categoryId = category.Id, categoryName = category.Name });
            }
            return Json(new { success = false, message = "Invalid data." });
        }   


    }
}
