using CoffeeManagement.Models;
using CoffeeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace CoffeeManagement.Controllers
{
    [Authorize(Roles ="admin,manager")]
    public class EmployeeController : Controller
    {
        private readonly Gol82750Ecoffee1Context _context;

        public EmployeeController(Gol82750Ecoffee1Context context)
        {
            _context = context;
        }



        // Index: Hiển thị danh sách nhân viên
        public IActionResult Index()
        {
            return View();
        }


        public async Task<IActionResult> _GetList(int start = 0, int length = 10, string keyword = "")
        {

            var employeesQuery = _context.Employees
                .Include(x => x.Status)
                .Include(x => x.Position)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                employeesQuery = employeesQuery.Where(e => e.FullName.Contains(keyword) || e.Email.Contains(keyword));
            }

            var totalRecords = await employeesQuery.CountAsync();

            var employees = await employeesQuery
                .Skip(start)
                .Take(length)
                .Select(x => new EmployeeViewModel(x))
                .ToListAsync();

            return Json(new
            {
                draw = Request.Query["draw"], // DataTable yêu cầu parameter 'draw' để đồng bộ các yêu cầu
                recordsTotal = totalRecords,  // Tổng số bản ghi trong cơ sở dữ liệu
                recordsFiltered = totalRecords, // Số bản ghi thỏa mãn điều kiện tìm kiếm
                data = employees // Dữ liệu trả về
            });
        }


        // Create: Hiển thị trang thêm mới nhân viên
        public IActionResult Create()
        {
            ViewBag.Positions = new SelectList(_context.EmployeePositions.AsNoTracking().ToList(), "Id", "Name");
            ViewBag.UserGroups = new SelectList(_context.UserGroups.AsNoTracking().ToList(), "Id", "Name");
            ViewBag.NewId = _context.Employees.AsNoTracking().OrderByDescending(x => x.Id).FirstOrDefault()?.Id + 1;
            return View();
        }

        // Create (POST): Lưu nhân viên mới vào cơ sở dữ liệu
        [HttpPost]
        public async Task<IActionResult> Create(Employee employee, string Username, string Password, int UserGroupId)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    employee.StatusId = 1;
                    if (employee.HasAccount)
                    {
                        if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
                        {
                            if(_context.Users.Any(x => x.Username == Username))
                            {
                                return Json(new { success = false, error = "Username is already exist!" });
                            }
                            employee.User = new User
                            {
                                Username = Username,
                                Password = Password,
                                UserGroupId = UserGroupId,
                                FullName = employee.FullName,
                                DayOfBirth = employee.DayOfBirth,
                                CreatedDate = DateTime.Now,
                                IsBlock = false,
                            };
                        }
                        else
                        {
                            return Json(new { success = false, error = "Username and Password are empty!" });
                        }
                    }
                    _context.Add(employee);
                    await _context.SaveChangesAsync();

                    return Json(new { success = true, message = "Employee created successfully!", redirect = Url.Action("Index", "Employee") });
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi nếu có, có thể log lỗi chi tiết vào hệ thống log
                    return Json(new { success = false, error = $"Error: {ex.Message}" });
                }
            }
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                   .Select(e => e.ErrorMessage)
                                   .ToList();
            return Json(new { success = false, error = $"Model state is not valid. {string.Join(", ", errors)}" });
        }


        // Edit: Hiển thị trang chỉnh sửa thông tin nhân viên
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(x => x.User)
                .SingleOrDefaultAsync(x => x.Id == id);
            if (employee == null)
            {
                return NotFound();
            }
            ViewBag.Positions = new SelectList(_context.EmployeePositions.AsNoTracking().ToList(), "Id", "Name", employee.PositionId);
            ViewBag.UserGroups = new SelectList(_context.UserGroups.AsNoTracking().ToList(), "Id", "Name", employee.User?.UserGroupId);
            ViewBag.Statuses = new SelectList(_context.EmployeeStatuses.AsNoTracking().ToList(), "Id", "Name", employee.StatusId);
            return View(employee);
        }

        // Edit (POST): Lưu các thay đổi vào cơ sở dữ liệu
        [HttpPost]
        public async Task<IActionResult> Edit(Employee employee, string Username, string Password, long UserGroupId)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existModel = await _context.Employees.FindAsync(employee.Id);
                    if (existModel == null)
                    {
                        return Json(new { success = false, error = "Employee not found." });
                    }
                    if (employee.HasAccount)
                    {
                        var existAccount = await _context.Users.FindAsync(employee.UserId);
                        if(existAccount == null)
                        {
                            existModel.User = (new User
                            {
                                Username = Username,
                                Password = Password,
                                UserGroupId = UserGroupId,
                                FullName = employee.FullName,
                                DayOfBirth = employee.DayOfBirth,
                                CreatedDate = DateTime.Now,
                                IsBlock = false,
                            });
                        }
                        else
                        {
                            existAccount.Username = Username;
                            existAccount.Password = Password;
                            existAccount.UserGroupId = UserGroupId;
                        }
                    }
                    else
                    {
                        var existAccount = await _context.Users.FindAsync(employee.UserId);
                        if(existAccount != null)
                        {
                            _context.Users.Remove(existAccount);
                        }
                    }
                    existModel.FullName = employee.FullName;
                    existModel.DayOfBirth = employee.DayOfBirth;
                    existModel.HiredDate = DateTime.Now;
                    existModel.Address = employee.Address;
                    existModel.Email = employee.Email;
                    existModel.PhoneNo = employee.PhoneNo;
                    existModel.HasAccount = employee.HasAccount;
                    existModel.PositionId = employee.PositionId;
                    existModel.StatusId = employee.StatusId;
                    existModel.Gender = employee.Gender;
                    existModel.SalaryPerHour = employee.SalaryPerHour;
                    if(existModel.StatusId == 2)
                    {
                        existModel.FiredDate = DateTime.Now;
                    }

                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Employee updated successfully!", redirect=Url.Action("Index") });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Id))
                    {
                        return Json(new { success = false, error = "Employee not found." });
                    }
                    else
                    {
                        // Xử lý lỗi cập nhật
                        return Json(new { success = false, error = "Concurrency error occurred while updating." });
                    }
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi tổng quát
                    return Json(new { success = false, error = $"Error: {ex.Message}" });
                }
            }

            return Json(new { success = false, error = "Model state is not valid." });
        }



        // Delete (POST): Xóa nhân viên khỏi cơ sở dữ liệu
        [HttpPost]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(id);
                if (employee != null)
                {
                    var user = await _context.Users.FindAsync(employee.UserId);
                    _context.Employees.Remove(employee);
                    if (user != null)
                    {
                        _context.Users.Remove(user);
                    }
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Xóa thành công" });
                }
                else
                {
                    return Json(new { success = false, error = "Không tìm thấy nhân viên" });
                }
            }
            catch(Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        private bool EmployeeExists(long id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
}
