using CoffeeManagement.Models;
using CoffeeManagement.Services;
using CoffeeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CoffeeManagement.Controllers
{
    public class CustomerController : Controller
    {
        private readonly Gol82750Ecoffee1Context _context;
        private readonly CustomerService _customerService;
        public CustomerController(Gol82750Ecoffee1Context context)
        {
            _context = context;
            _customerService = new CustomerService(context);
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> _GetList(int start = 0, int length = 25, string keyword = "")
        {
            var draw = Request.Form["draw"].ToString();
            var query = _context.Customers
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(e => (e.FirstName != null && e.FirstName.Contains(keyword)) || (e.LastName != null && e.LastName.Contains(keyword)) || (e.CustomerNo != null && e.CustomerNo.Contains(keyword)));
            }

            var totalRecords = await query.CountAsync();

            var list = await query
                .Skip(start)
                .Take(length)
                .Select(x => new
                {
                    Id = x.Id,
                    CustomerNo = x.CustomerNo,
                    Name = x.FirstName + " " + x.LastName,
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
            ViewBag.CustomerNo = _customerService.GenerateNewCustomerNo();
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(Customer customer)
        {
            try
            {
                if (string.IsNullOrEmpty(customer.CustomerNo) || string.IsNullOrEmpty(customer.FirstName))
                {
                    return Json(new { success = false, message = "Customer code and first name are required." });
                }
                long userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                customer.CustomerNo = _customerService.GenerateNewCustomerNo();
                customer.CreatedAt = DateTime.Now;
                customer.CreatedByUserId = userId;

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Customer created successfully.",
                    customerId = customer.Id,
                    customerNo = customer.CustomerNo,
                    firstName = customer.FirstName,
                    lastName = customer.LastName
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        public async Task<IActionResult> Edit(long id)
        {
            var model = await _context.Customers.FindAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(long id, Customer model)
        {
            try
            {
                var exist = await _context.Customers.FindAsync(id);
                if (exist == null)
                {
                    return NotFound();
                }

                exist.CustomerNo = model.CustomerNo;
                exist.FirstName = model.FirstName;
                exist.Address = model.Address;
                exist.LastName = model.LastName;
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


        [HttpPost]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var exist = await _context.Customers.FindAsync(id);
                if (exist == null)
                {
                    return NotFound();
                }
                _context.Customers.Remove(exist);
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
