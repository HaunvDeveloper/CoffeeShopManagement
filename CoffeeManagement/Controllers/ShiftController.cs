using CoffeeManagement.Models;
using CoffeeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CoffeeManagement.Controllers
{
    [Authorize]
    public class ShiftController : Controller
    {
        private readonly Gol82750Ecoffee1Context _context;

        public ShiftController(Gol82750Ecoffee1Context context)
        {
            _context = context;
        }

        [Authorize(Roles ="admin,manager")]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Register()
        {
            DateTime nextMonday = DateTime.Now.AddDays(((int)DayOfWeek.Monday - (int)DateTime.Now.DayOfWeek + 7) % 7);
            DateTime nextSunday = nextMonday.AddDays(6);

            ViewBag.NextMonday = nextMonday.ToString("yyyy-MM-dd");
            ViewBag.NextSunday = nextSunday.ToString("yyyy-MM-dd");
            ViewBag.Shifts = _context.Shifts.AsNoTracking().ToList();

            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var employee = _context.Employees.FirstOrDefault(x => x.UserId == userId);
            if (employee != null)
            {
                DateOnly nextMondayDateOnly = DateOnly.FromDateTime(nextMonday);
                DateOnly nextSundayDateOnly = DateOnly.FromDateTime(nextSunday);

                var registered = _context.ShiftRegistrations
                    .Include(x => x.ShiftConfirmations)
                    .Where(x => x.WorkDate >= nextMondayDateOnly && x.WorkDate <= nextSundayDateOnly && x.EmployeeId == employee.Id)
                    .ToList();


                ViewBag.Registered = registered;
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(List<ShiftRegistration> data, List<ShiftRegistration> removeList)
        {
            try
            {
                int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var employee = _context.Employees.FirstOrDefault(x => x.UserId == userId);
                if(employee == null)
                {
                    return Json(new { success = false, error = "Employee not found!" });
                }
                foreach (var registration in data)
                {
                    registration.EmployeeId = employee.Id;
                    registration.IsApproved = null;
                    if(!_context.ShiftRegistrations.Any(x => x.WorkDate == registration.WorkDate && x.ShiftId == registration.ShiftId && x.EmployeeId == employee.Id))
                    {
                        _context.ShiftRegistrations.Add(registration);
                    }
                }
                _context.ShiftRegistrations.RemoveRange(removeList);
                await _context.SaveChangesAsync();
                

                return Json(new { success = true, message = "Lưu thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message, detailError = ex.ToString() });
            }
        }
        
        
        [Authorize(Roles = "admin,manager")]
        public IActionResult Checking()
        {
            DateTime nextMonday = DateTime.Now.AddDays(((int)DayOfWeek.Monday - (int)DateTime.Now.DayOfWeek + 7) % 7);
            DateTime nextSunday = nextMonday.AddDays(6);
            ViewBag.NextMonday = nextMonday.ToString("yyyy-MM-dd");
            ViewBag.NextSunday = nextSunday.ToString("yyyy-MM-dd");

            return View();
        }


        [Authorize(Roles = "admin,manager")]
        public IActionResult _GetListChecking()
        {
            var shifts = _context.Shifts.AsNoTracking().ToList();

            var shiftRegistrations = _context.ShiftRegistrations
                .AsNoTracking()
                .Include(sr => sr.Employee)
                    .ThenInclude(emp => emp.Position)
                .Include(sr => sr.Shift)
                .Select(x => new WorkDayListViewModel
                {
                    RegisterId = x.Id,
                    ShiftId = x.ShiftId,
                    WorkDate = x.WorkDate,
                    Employee = new Employee
                    {
                        Id = x.Employee.Id,
                        FullName = x.Employee.FullName,
                        DayOfBirth = x.Employee.DayOfBirth,
                        PositionId = x.Employee.Id,
                        Position = x.Employee.Position,
                    },
                    IsApproved = x.IsApproved,
                }
                )
                .ToList();

            ViewBag.Shifts = shifts;

            ViewBag.Registrations = shiftRegistrations;

            DateTime nextMonday = DateTime.Now.AddDays(((int)DayOfWeek.Monday - (int)DateTime.Now.DayOfWeek + 7) % 7);
            DateTime nextSunday = nextMonday.AddDays(6);
            ViewBag.NextMonday = nextMonday.ToString("yyyy-MM-dd");
            ViewBag.NextSunday = nextSunday.ToString("yyyy-MM-dd");
            return PartialView();
        }

        [Authorize(Roles = "admin,manager")]
        public IActionResult _GetListRegister(long shiftId, DateOnly date)
        {
            
            var shiftRegistrations = _context.ShiftRegistrations
                .Include(sr => sr.Employee)
                    .ThenInclude(emp => emp.Position)
                .Include(sr => sr.Shift)
                .Include(sr => sr.ShiftConfirmations)
                .Where(sr => sr.WorkDate == date && sr.ShiftId == shiftId)
                .ToList();
            return PartialView( shiftRegistrations );
        }


        [Authorize(Roles = "admin,manager")]
        [HttpPost]
        public async Task<IActionResult> ApprovedRegister(List<ShiftConfirmation> confirmations)
        {
            try
            {
                if(confirmations == null || !confirmations.Any())
                {
                    return Json(new { success = false, error = "Dữ liệu trống" });
                }
                int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var user = _context.Users.AsNoTracking().SingleOrDefault(x => x.Id == userId);
                var listWorkSchedule = new List<WorkSchedule>();
                foreach (ShiftConfirmation confirmation in confirmations)
                {
                    var register = await _context.ShiftRegistrations.FindAsync(confirmation.RegistrationId);
                    if(register != null)
                    {
                        register.IsApproved = confirmation.IsApproved;
                        if(register.IsApproved == false)
                        {
                            register.IsForce = false;
                        }
                    }
                    confirmation.ApprovedAt = DateTime.Now;
                    confirmation.ApprovedByUserId = userId;
                    confirmation.ApprovedbyUsername = user?.Username;
                    if (confirmation.IsApproved == true)
                    {
                        // Tạo WorkSchedule nếu được phê duyệt
                        var workSchedule = new WorkSchedule
                        {
                            EmployeeId = register.EmployeeId,
                            ShiftId = register.ShiftId,
                            WorkDate = register.WorkDate,
                            CreatedAt = DateTime.Now,
                            CreatedByUserId = userId,
                            Status = "CHƯA HOÀN THÀNH",
                            IsDone = false,
                            IsLate = false,
                            LateTime = null,
                            CheckinTime = null,
                            CheckoutTime = null
                        };
                        listWorkSchedule.Add(workSchedule);
                    }
                }

                // Lưu WorkSchedule vào cơ sở dữ liệu
                if (listWorkSchedule.Any())
                {
                    _context.WorkSchedules.AddRange(listWorkSchedule);
                }
                _context.ShiftConfirmations.AddRange( confirmations );
                await _context.SaveChangesAsync();
                return Json(new { success = true, message="Lưu thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message, detailError=ex.ToString() });
            }
        }



        [Authorize(Roles = "admin,manager")]
        public IActionResult Manage()
        {
            return View();
        }

        [Authorize(Roles = "admin,manager")]
        public IActionResult _GetListManage(DateOnly monday, DateOnly sunday)
        {
            var shifts = _context.Shifts.AsNoTracking().ToList();

            var shiftRegistrations = _context.ShiftRegistrations
                .AsNoTracking()
                .Include(sr => sr.Employee)
                    .ThenInclude(emp => emp.Position)
                .Include(sr => sr.Shift)
                .Where(sr => sr.WorkDate >= monday && sr.WorkDate <= sunday)
                .Select(x => new WorkDayListViewModel
                {
                    RegisterId = x.Id,
                    ShiftId = x.ShiftId,
                    WorkDate = x.WorkDate,
                    Employee = new Employee
                    {
                        Id = x.Employee.Id,
                        FullName = x.Employee.FullName,
                        DayOfBirth = x.Employee.DayOfBirth,
                        PositionId = x.Employee.Id,
                        Position = x.Employee.Position,
                    },
                    IsApproved = x.IsApproved,
                }
                )
                .ToList();

            ViewBag.Shifts = shifts;
            ViewBag.Registrations = shiftRegistrations;
            ViewBag.NextMonday = monday.ToString("yyyy-MM-dd");
            ViewBag.NextSunday = sunday.ToString("yyyy-MM-dd");
            return PartialView();
        }

        [Authorize(Roles = "admin,manager")]
        public IActionResult _GetListEmployeeManage(long shiftId, DateOnly date)
        {
            var shiftsRegist = _context.ShiftRegistrations
                .AsNoTracking()
                .Where(x => x.WorkDate == date && x.ShiftId == shiftId)
                .ToList();

            var approvedRegist = shiftsRegist.Where(x => x.IsApproved == true).Select(x => x.EmployeeId).ToList();
            var registered = shiftsRegist.Where(x => x.IsApproved != true).Select(x => x.EmployeeId).ToList();
            var deniedRegist = shiftsRegist.Where(x => x.IsApproved == false).Select(x => x.EmployeeId).ToList();
            var employees = _context.Employees.AsNoTracking()
                                .Include(x => x.Position)
                                .Select(e => new EmployeeShiftCheckingViewModel
                                {
                                    Id = e.Id,
                                    FullName = e.FullName,
                                    PhoneNo = e.PhoneNo,
                                    DayOfBirth = e.DayOfBirth,
                                    Gender = e.Gender,
                                    PositionName = e.Position.Name,
                                    StatusRegist =  deniedRegist.Contains(e.Id) ? "Đã bị từ chối" : registered.Contains(e.Id) ? "Đang chờ duyệt" : "",
                                    IsRegistered = registered.Contains(e.Id),
                                    IsWork = approvedRegist.Contains(e.Id)
                                }).ToList();
            
            
            return PartialView(employees);
        }

        [HttpPost]
        [Authorize(Roles = "admin,manager")]
        public IActionResult SaveShiftRegistrations(List<long> listAddNew, List<long> listDelete, long shiftId, DateOnly workDate)
        {
            if (listAddNew == null && listDelete == null)
            {
                return Json(new { success = false, message = "Không có nhân viên nào để cập nhật." });
            }

            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = _context.Users.AsNoTracking().SingleOrDefault(x => x.Id == userId);
            if (user == null)
            {
                return Unauthorized();
            }

            // Xử lý thêm nhân viên mới vào ShiftRegistrations
            if (listAddNew != null && listAddNew.Any())
            {
                // Kiểm tra các bản ghi đã tồn tại với IsApproved = false
                var existingRegistrations = _context.ShiftRegistrations
                    .Where(sr => sr.ShiftId == shiftId && sr.WorkDate == workDate && listAddNew.Contains(sr.EmployeeId))
                    .ToList();

                foreach (var existingRegistration in existingRegistrations)
                {
                    if (existingRegistration.IsApproved != true)
                    {
                        // Nếu bản ghi đã tồn tại và IsApproved = false, cập nhật IsApproved = true
                        existingRegistration.IsApproved = true;
                        existingRegistration.ShiftConfirmations.Add(new ShiftConfirmation
                        {
                            IsApproved = true,
                            ApprovedAt = DateTime.Now,
                            ApprovedByUserId = user.Id,
                            ApprovedbyUsername = user.Username,
                        });

                        // Thêm WorkSchedule nếu chưa tồn tại
                        if (!_context.WorkSchedules.Any(ws => ws.EmployeeId == existingRegistration.EmployeeId && ws.ShiftId == shiftId && ws.WorkDate == workDate))
                        {
                            var workSchedule = new WorkSchedule
                            {
                                EmployeeId = existingRegistration.EmployeeId,
                                ShiftId = shiftId,
                                WorkDate = workDate,
                                CreatedAt = DateTime.Now,
                                CreatedByUserId = user.Id,
                                Status = "CHƯA HOÀN THÀNH",
                                IsDone = false,
                                IsLate = false,
                                LateTime = null,
                                CheckinTime = null,
                                CheckoutTime = null
                            };
                            _context.WorkSchedules.Add(workSchedule);
                        }

                    }

                    // Xóa nhân viên đã được xử lý khỏi listAddNew để tránh thêm trùng lặp
                    listAddNew.Remove(existingRegistration.EmployeeId);
                }

                // Thêm các nhân viên còn lại chưa tồn tại vào ShiftRegistrations
                var newRegistrations = listAddNew.Select(employeeId => new ShiftRegistration
                {
                    EmployeeId = employeeId,
                    ShiftId = shiftId,
                    WorkDate = workDate,
                    IsApproved = true,
                    IsForce = true,
                    CreatedAt = DateTime.Now,
                    ShiftConfirmations = new List<ShiftConfirmation>
                    {
                        new ShiftConfirmation
                        {
                            IsApproved = true,
                            ApprovedAt = DateTime.Now,
                            ApprovedByUserId = user.Id,
                            ApprovedbyUsername = user.Username,
                        }
                    }
                }).ToList();

                // Thêm WorkSchedule cho các nhân viên mới
                foreach (var registration in newRegistrations)
                {
                    if (!_context.WorkSchedules.Any(ws => ws.EmployeeId == registration.EmployeeId && ws.ShiftId == shiftId && ws.WorkDate == workDate))
                    {
                        var workSchedule = new WorkSchedule
                        {
                            EmployeeId = registration.EmployeeId,
                            ShiftId = shiftId,
                            WorkDate = workDate,
                            CreatedAt = DateTime.Now,
                            CreatedByUserId = user.Id,
                            Status = "CHƯA HOÀN THÀNH",
                            IsDone = false,
                            IsLate = false,
                            LateTime = null,
                            CheckinTime = null,
                            CheckoutTime = null
                        };
                        _context.WorkSchedules.Add(workSchedule);
                    }
                }

                _context.ShiftRegistrations.AddRange(newRegistrations);
            }

            // Xử lý xóa nhân viên khỏi ShiftRegistrations
            if (listDelete != null && listDelete.Any())
            {
                var deniedRegist = _context.ShiftRegistrations
                    .Where(sr => listDelete.Contains(sr.EmployeeId) && sr.ShiftId == shiftId && sr.WorkDate == workDate)
                    .ToList();
                var deniedConfirm = new List<ShiftConfirmation>();
                var workSchedulesToDelete = _context.WorkSchedules
                    .Where(ws => listDelete.Contains(ws.EmployeeId) && ws.ShiftId == shiftId && ws.WorkDate == workDate)
                    .ToList();
                foreach (var regist in deniedRegist)
                {
                    regist.IsApproved = false;
                    regist.IsForce = false;
                    deniedConfirm.Add(new ShiftConfirmation
                    {
                        RegistrationId = regist.Id,
                        IsApproved = false,
                        ApprovedAt = DateTime.Now,
                        ApprovedByUserId= user.Id,
                        ApprovedbyUsername = user.Username,
                        RejectReason = "Reject by Manage Service",
                    });
                }

                // Xóa các WorkSchedule tương ứng
                if (workSchedulesToDelete.Any())
                {
                    _context.WorkSchedules.RemoveRange(workSchedulesToDelete);
                }
                _context.ShiftConfirmations.AddRange(deniedConfirm);
            }

            // Lưu thay đổi vào cơ sở dữ liệu
            try
            {
                _context.SaveChanges();
                return Json(new { success = true, message = "Cập nhật thành công!" });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi khi lưu vào cơ sở dữ liệu
                return Json(new { success = false, message = "Lỗi khi lưu thay đổi: " + ex.Message });
            }
        }




    }
}
