using CoffeeManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CoffeeManagement.Controllers
{
    [Authorize]
    public class AttendanceController : Controller
    {
        private readonly ILogger<AttendanceController> _logger;
        private readonly Gol82750Ecoffee1Context _context;
        private readonly List<string> _allowedIps;
        public AttendanceController(ILogger<AttendanceController> logger, Gol82750Ecoffee1Context context, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _allowedIps = configuration.GetSection("AllowedIps").Get<List<string>>() ?? new List<string>();
        }

        public IActionResult Index()
        {
            return View();
        }


        public async Task<IActionResult> _GetIncommingWorkSchedule()
        {
            long userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == userId);

            if (employee == null)
            {
                return NotFound("Employee not found.");
            }

            var currentTime = TimeOnly.FromDateTime(DateTime.Now);


            var workSchedulesRaw = await _context.WorkSchedules
                .Include(ws => ws.Shift)
                .Where(ws =>
                    ws.WorkDate == DateOnly.FromDateTime(DateTime.Now) &&
                    ws.Shift.StartTime.AddMinutes(-30) <= currentTime &&
                    ws.Shift.EndTime >= currentTime &&
                    ws.EmployeeId == employee.Id &&
                    ws.CheckinTime == null // Chỉ lấy những lịch làm việc chưa check-in
                )
                .ToListAsync(); // Execute query on DB

            // Xử lý phía client
            var workSchedules = workSchedulesRaw
                .Select(ws => new
                {
                    ws.Id,
                    ws.WorkDate,
                    StartTime = ws.Shift.StartTime.ToString("HH:mm:ss"),
                    EndTime = ws.Shift.EndTime.ToString("HH:mm:ss"),
                    ShiftName = ws.Shift.Name,
                    ws.EmployeeId,
                    RemainingTime = ws.Shift.StartTime < currentTime ?  currentTime - ws.Shift.StartTime : ws.Shift.StartTime - currentTime,
                    IsLate = ws.Shift.StartTime < currentTime
                })
                .OrderBy(ws => ws.RemainingTime)
                .ToList();

            return Json(new { data = workSchedules });
        }

        public async Task<IActionResult> _GetOutGoingWorkSchedule()
        {
            long userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == userId);

            if (employee == null)
            {
                return NotFound("Employee not found.");
            }

            var currentTime = TimeOnly.FromDateTime(DateTime.Now); // Lấy thời gian hiện tại
            var workSchedulesRaw = await _context.WorkSchedules
                .Include(ws => ws.Shift)
                .Where(ws =>
                    ws.WorkDate == DateOnly.FromDateTime(DateTime.Now) &&
                    ws.Shift.EndTime <= currentTime &&
                    ws.Shift.EndTime.AddHours(3) >= currentTime &&
                    ws.EmployeeId == employee.Id &&
                    ws.CheckoutTime == null // Chỉ lấy những lịch làm việc chưa check-out
                )
                .ToListAsync();

            var workSchedules = workSchedulesRaw
                .Select(ws => new
                {
                    ws.Id,
                    ws.WorkDate,
                    StartTime = ws.Shift.StartTime.ToString("HH:mm:ss"),
                    EndTime = ws.Shift.EndTime.ToString("HH:mm:ss"),
                    ShiftName = ws.Shift.Name,
                    ws.EmployeeId,
                    Overtime = currentTime - ws.Shift.EndTime,
                    IsOvertime = currentTime > ws.Shift.EndTime
                })
                .OrderBy(ws => ws.Overtime)
                .ToList();

            return Json(new { data = workSchedules });
        }

        [HttpPost]
        public async Task<IActionResult> Checkin(long id)
        {
            if (!IsFromCafeNetwork(HttpContext))
            {
                return Json(new { success = false, message = "You must be connected to cafe's wifi to check in." });
            }
            long userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == userId);
            if (employee == null)
            {
                return NotFound("Employee not found.");
            }
            var workSchedule = await _context.WorkSchedules
                .Include(x => x.Shift)
                .FirstOrDefaultAsync(ws => ws.Id == id && ws.EmployeeId == employee.Id && ws.CheckinTime == null);
            if (workSchedule == null)
            {
                return NotFound("Work schedule not found or already checked in.");
            }

            // Kiểm tra xem đã đến giờ check-in chưa
            var currentTime = TimeOnly.FromDateTime(DateTime.Now);
            if (currentTime < workSchedule.Shift.StartTime.AddMinutes(-30) || currentTime > workSchedule.Shift.EndTime)
            {
                return Json(new { success = false, message = "You can only check in 30 minutes before your shift starts." });
            }



            workSchedule.CheckinTime = DateTime.Now;

            workSchedule.IsLate = currentTime > workSchedule.Shift.StartTime;
            if (workSchedule.IsLate)
            {
                workSchedule.LateTime =  currentTime.AddMinutes(-workSchedule.Shift.StartTime.Minute);
                workSchedule.Status = "Late";
            }
            else
            {
                workSchedule.LateTime = null;
                workSchedule.Status = "On Time";
            }

            _context.WorkSchedules.Update(workSchedule);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Check-in successful." });
        }


        [HttpPost]
        public async Task<IActionResult> Checkout(long id)
        {
            if (!IsFromCafeNetwork(HttpContext))
            {
                return Json(new { success = false, message = "You must be connected to cafe's wifi to check in." });
            }
            long userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == userId);
            if (employee == null)
            {
                return NotFound("Employee not found.");
            }
            var workSchedule = await _context.WorkSchedules
                .Include(x => x.Shift)
                .FirstOrDefaultAsync(ws => ws.Id == id && ws.EmployeeId == employee.Id && ws.CheckoutTime == null);
            if (workSchedule == null)
            {
                return NotFound("Work schedule not found or already checked out.");
            }
            // Kiểm tra xem đã đến giờ check-out chưa
            var currentTime = TimeOnly.FromDateTime(DateTime.Now);
            if (currentTime < workSchedule.Shift.EndTime || currentTime > workSchedule.Shift.EndTime.AddHours(3))
            {
                return Json(new { success = false, message = "You can only check out within 3 hours after your shift ends." });
            }
            workSchedule.CheckoutTime = DateTime.Now;
            workSchedule.IsDone = true; // Đánh dấu là đã hoàn thành công việc

            if(workSchedule.CheckinTime == null)
            {
                workSchedule.IsLate = true; // Nếu chưa check-in thì đánh dấu là muộn
                workSchedule.LateTime = currentTime.AddMinutes(-workSchedule.Shift.StartTime.Minute);
            }

            _context.WorkSchedules.Update(workSchedule);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Check-out successful." });
        }

        private bool IsFromCafeNetwork(HttpContext context)
        {
            var remoteIp = context.Connection.RemoteIpAddress?.ToString();
            Console.WriteLine($"\n\nRemote IP: {remoteIp}\n\n");
            if (string.IsNullOrEmpty(remoteIp)) return false;

            // So khớp full hoặc bắt đầu bằng pattern trong cấu hình
            return _allowedIps.Any(ip => remoteIp.StartsWith(ip));
        }


    }
}
