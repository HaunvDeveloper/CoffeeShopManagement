using CoffeeManagement.Models;
using CoffeeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CoffeeManagement.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly Gol82750Ecoffee1Context _context;

        public ProductController(Gol82750Ecoffee1Context context)
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
            var query = _context.Products
                .Include(x => x.Category)
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
                .Select(x => new ProductViewModel
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    StockQuantity = x.StockQuantity,
                    Unit = x.Unit,
                    Vat = x.Vat,
                    CreatedDateStr = x.CreatedDate.ToShortDateString(),
                    Price = x.Price,
                    ImageBinary = x.ImageBinary != null ? Convert.ToBase64String(x.ImageBinary) : "",
                    Description = x.Description,
                    CategoryName = x.Category != null ? x.Category.Name : "",
                    Origin = x.Origin,
                    IsActive = x.IsActive,
                })
                .ToListAsync();

            return Json(new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = totalRecords,
                data = list
            });
        }

        public IActionResult Create()
        {
            ViewBag.CategoryList = _context.Categories
                .AsNoTracking()
                .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name })
                .ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product, IFormFile ImageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (ImageFile != null)
                    {
                        if (!ImageFile.ContentType.StartsWith("image/"))
                        {
                            return Json(new { success = false, message = "Please upload a valid image file." });
                        }

                        if (ImageFile.Length > 5 * 1024 * 1024)
                        {
                            return Json(new { success = false, message = "Image file is too large. Maximum size is 5MB." });
                        }

                        using var memoryStream = new MemoryStream();
                        await ImageFile.CopyToAsync(memoryStream);
                        product.ImageBinary = memoryStream.ToArray();
                    }

                    product.CreatedDate = DateTime.Now;
                    _context.Products.Add(product);
                    await _context.SaveChangesAsync();

                    return Json(new { success = true, message = "Tạo thành công", redirect = Url.Action("Index") });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
                }
            }

            // Nếu ModelState không hợp lệ, trả về lỗi
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(new { success = false, message = "Validation failed: " + string.Join("; ", errors) });
        }

        public async Task<IActionResult> Edit(long id)
        {
            var model = await _context.Products.FindAsync(id);
            if (model == null)
            {
                return NotFound();
            }
            ViewBag.CategoryList = _context.Categories
                .AsNoTracking()
                .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name })
                .ToList();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Product product, IFormFile? ImageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = await _context.Products.FindAsync(product.Id);
                    if (existingProduct == null)
                    {
                        return Json(new { success = false, message = "Product not found." });
                    }

                    if (ImageFile != null)
                    {
                        if (!ImageFile.ContentType.StartsWith("image/"))
                        {
                            return Json(new { success = false, message = "Please upload a valid image file." });
                        }

                        if (ImageFile.Length > 5 * 1024 * 1024)
                        {
                            return Json(new { success = false, message = "Image file is too large. Maximum size is 5MB." });
                        }

                        using var memoryStream = new MemoryStream();
                        await ImageFile.CopyToAsync(memoryStream);
                        existingProduct.ImageBinary = memoryStream.ToArray();
                    }

                    existingProduct.Code = product.Code;
                    existingProduct.Name = product.Name;
                    existingProduct.Unit = product.Unit;
                    existingProduct.Price = product.Price;
                    existingProduct.StockQuantity = product.StockQuantity;
                    existingProduct.CategoryId = product.CategoryId;
                    existingProduct.Origin = product.Origin;
                    existingProduct.IsActive = product.IsActive;
                    existingProduct.Description = product.Description;
                    existingProduct.UpdatedDate = DateTime.Now;
                    existingProduct.Vat = product.Vat;

                    _context.Update(existingProduct);
                    await _context.SaveChangesAsync();

                    return Json(new { success = true ,message="Lưu thành công", redirect = Url.Action("Index") });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
                }
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(new { success = false, message = "Validation failed: " + string.Join("; ", errors) });
        }

        [HttpPost]
        public async Task<IActionResult> GetCode(long categoryId)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            if (category == null)
            {
                return Json(new { success = false, message = "Category not found." });
            }
            var code = category.Code;
            var lastProduct = await _context.Products
                .Where(p => p.Code.StartsWith(code))
                .OrderByDescending(p => p.Code)
                .FirstOrDefaultAsync();
            if (lastProduct != null)
            {
                var lastCode = lastProduct.Code;
                var lastNumber = int.Parse(lastCode.Substring(code.Length));
                code += (lastNumber + 1).ToString("D3");
            }
            else
            {
                code += "001"; // Default to 001 if no products found
            }
            return Json(new { success = true, code = code });
        }
    }
}
