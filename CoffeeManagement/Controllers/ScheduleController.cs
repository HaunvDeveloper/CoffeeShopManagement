using CoffeeManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CoffeeManagement.Controllers
{
    [Authorize]
    public class ScheduleController : Controller
    {
        private readonly Gol82750Ecoffee1Context _context;

        public ScheduleController(Gol82750Ecoffee1Context context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            
            return View();
        }

        public IActionResult _GetSchedules(DateTime startDate, DateTime endDate)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var employee = _context.Employees.FirstOrDefault(x => x.UserId == userId);
            if (employee == null)
            {
                return NotFound();
            }
            DateOnly sd = new DateOnly(startDate.Year, startDate.Month, startDate.Day);
            DateOnly ed = new DateOnly(endDate.Year, endDate.Month, endDate.Day);
            ViewBag.StartDate = sd;
            ViewBag.EndDate = ed;
            ViewBag.Shifts = _context.Shifts.AsNoTracking().ToList();
            var list = _context.WorkSchedules.AsNoTracking()
                .Include(x => x.Shift)
                .Where(x => x.WorkDate >= sd && x.WorkDate <= ed && x.EmployeeId == employee.Id)
                .ToList();
            return PartialView(list);
        }



        private List<(DateTime a, DateTime b)> GetWeeks(DateTime startDate, DateTime endDate)
        {
            List<(DateTime a, DateTime b)> weeks = new List<(DateTime a, DateTime b)>();

            // Tìm ngày thứ Hai đầu tiên trong tuần của startDate
            DateTime startOfWeek = startDate.AddDays(DayOfWeek.Monday - startDate.DayOfWeek);
            if (startDate.DayOfWeek != DayOfWeek.Monday)
            {
                startOfWeek = startOfWeek.AddDays(-7); // Nếu startDate không phải là thứ Hai, lấy ngày thứ Hai của tuần tiếp theo
            }

            // Tìm ngày Chủ Nhật cuối cùng trong tuần của endDate
            DateTime endOfWeek = endDate.AddDays(DayOfWeek.Sunday - endDate.DayOfWeek);

            // Lặp qua từng tuần từ startDate đến endDate
            for (DateTime current = startOfWeek; current <= endOfWeek; current = current.AddDays(7))
            {
                DateTime monday = current;
                DateTime sunday = monday.AddDays(6);

                // Nếu sunday > endDate, đặt sunday = endDate
                if (sunday > endDate)
                {
                    sunday = endDate;
                }

                // Thêm vào danh sách theo định dạng yêu cầu
                weeks.Add((monday, sunday));
            }

            return weeks;
        }
    }
}
