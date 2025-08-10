using CoffeeManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CoffeeManagement.Controllers
{
    public class DashboardController : Controller
    {
        private readonly Gol82750Ecoffee1Context _context;

        public DashboardController(Gol82750Ecoffee1Context context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetHeaderData()
        {
            var totalOrders = await _context.Orders.CountAsync();
            var totalCustomers = await _context.Customers.CountAsync();
            var totalProducts = await _context.Products.CountAsync();
            var totalRevenue = await _context.Orders
                .Where(o => o.IsPaid == true)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;
            return Json(new
            {
                totalOrders,
                totalCustomers,
                totalProducts,
                totalRevenue
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetMonthlySalesData()
        {
            var currentYear = DateTime.Now.Year;
            var monthNow = DateTime.Now.Month;
            var monthBefore = monthNow - 1;

            // 1. Doanh thu (Revenue)
            var salesData = await _context.Orders
                .Where(o => o.SaleDate.Year == currentYear && o.IsPaid == true)
                .GroupBy(o => o.SaleDate.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    TotalRevenue = g.Sum(o => o.TotalAmount)
                })
                .ToListAsync();

            // 2. Chi phí (Cost)
            var purchaseData = await _context.Purchases
                .Where(p => p.PurchasedDate.Year == currentYear)
                .GroupBy(p => p.PurchasedDate.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    TotalCost = g.Sum(p => p.TotalAmount)
                })
                .ToListAsync();

            // 3. Tính danh sách tháng
            var startMonth = Math.Max(1, monthNow - 4);
            var months = Enumerable.Range(startMonth, monthNow - startMonth + 1)
                .Select(m => $"{currentYear}-{m:00}")
                .ToArray();

            var revenue = months
                .Select(m => salesData.FirstOrDefault(s => s.Month.ToString("00") == m.Split('-')[1])?.TotalRevenue ?? 0)
                .ToArray();

            var cost = months
                .Select(m => purchaseData.FirstOrDefault(p => p.Month.ToString("00") == m.Split('-')[1])?.TotalCost ?? 0)
                .ToArray();

            // 4. Tổng theo tháng hiện tại
            var totalRevenueRaw = salesData.FirstOrDefault(x => x.Month == monthNow)?.TotalRevenue ?? 0;
            var totalCostRaw = purchaseData.FirstOrDefault(x => x.Month == monthNow)?.TotalCost ?? 0;
            var totalProfitRaw = totalRevenueRaw - totalCostRaw;

            // 5. Tổng theo tháng trước
            var prevRevenueRaw = salesData.FirstOrDefault(x => x.Month == monthBefore)?.TotalRevenue ?? 0;
            var prevCostRaw = purchaseData.FirstOrDefault(x => x.Month == monthBefore)?.TotalCost ?? 0;
            var prevProfitRaw = prevRevenueRaw - prevCostRaw;

            // 6. Hiệu suất tăng/giảm (%)
            decimal GetEfficiency(decimal current, decimal previous)
            {
                if (previous == 0)
                    return current == 0 ? 0 : 100;
                return ((current - previous) / previous) * 100;
            }

            var revenueEfficiency = GetEfficiency(totalRevenueRaw, prevRevenueRaw);
            var costEfficiency = GetEfficiency(totalCostRaw, prevCostRaw);
            var profitEfficiency = GetEfficiency(totalProfitRaw, prevProfitRaw);

            // 7. Format tiền
            var totalRevenue = totalRevenueRaw.ToString("C", new System.Globalization.CultureInfo("vi-VN"));
            var totalCost = totalCostRaw.ToString("C", new System.Globalization.CultureInfo("vi-VN"));
            var totalProfit = totalProfitRaw.ToString("C", new System.Globalization.CultureInfo("vi-VN"));

            // 8. Trả về kết quả
            return Json(new
            {
                categories = months,
                revenue,
                cost,
                totalRevenue,
                totalCost,
                totalProfit,
                revenueEfficiency,
                costEfficiency,
                profitEfficiency
            });
        }


        [HttpGet]
        public async Task<IActionResult> GetPieChartData()
        {
            var currentYear = DateTime.Now.Year;

            // Bước 1: Tính tổng doanh thu theo ProductId
            var allProductSales = await _context.OrderDetails
                .Include(od => od.Sale)
                .Where(od => od.Sale.SaleDate.Year == currentYear)
                .GroupBy(od => od.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalSales = g.Sum(od => od.Quantity * od.UnitPrice)
                })
                .OrderByDescending(g => g.TotalSales)
                .ToListAsync();

            // Bước 2: Lấy top 5 sản phẩm
            var top5 = allProductSales.Take(5).ToList();

            // Bước 3: Gộp phần còn lại vào "Other"
            var otherTotalSales = allProductSales.Skip(5).Sum(ps => ps.TotalSales);

            // Bước 4: Chuẩn bị series và labels
            var productIdsTop5 = top5.Select(ps => ps.ProductId).ToList();
            var productNames = await _context.Products
                .Where(p => productIdsTop5.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.Name);

            var series = top5.Select(ps => ps.TotalSales).ToList();
            var labels = top5.Select(ps => productNames.ContainsKey(ps.ProductId) ? productNames[ps.ProductId] : "Unknown").ToList();

            if (otherTotalSales > 0)
            {
                series.Add(otherTotalSales);
                labels.Add("Other");
            }

            return Json(new { series, labels });
        }


        [HttpGet]
        public async Task<IActionResult> GetVisitorsData()
        {
            var today = DateTime.Today;
            var startDate = today.AddDays(-6); // 7 ngày gần nhất

            var visitorData = await _context.OrderDetails
                .Include(od => od.Sale)
                .Where(o => o.Sale.SaleDate.Date >= startDate)
                .GroupBy(o => o.Sale.SaleDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    HighCount = g.Sum(x => x.Quantity),
                    LowCount = g.Count()
                })
                .ToListAsync();

            var dates = Enumerable.Range(0, 7)
                .Select(d => startDate.AddDays(d).ToString("dd'th'"))
                .ToArray();

            var highData = dates
                .Select(d => visitorData.FirstOrDefault(v => v.Date.ToString("dd'th'") == d)?.HighCount ?? 0)
                .ToArray();

            var lowData = dates
                .Select(d => visitorData.FirstOrDefault(v => v.Date.ToString("dd'th'") == d)?.LowCount ?? 0)
                .ToArray();

            // Tính số khách hôm nay
            var todayVisitors = visitorData.FirstOrDefault(v => v.Date == today)?.LowCount ?? 0;

            // Tìm đúng ngày cùng thứ của tuần trước
            var sameDayLastWeek = today.AddDays(-7);
            var lastWeekVisitors = await _context.OrderDetails
                .Include(od => od.Sale)
                .Where(o => o.Sale.SaleDate.Date == sameDayLastWeek)
                .CountAsync();

            // Tính hiệu suất
            decimal visitorsEfficiency;
            if (lastWeekVisitors == 0)
            {
                visitorsEfficiency = todayVisitors == 0 ? 0 : 100;
            }
            else
            {
                visitorsEfficiency = ((todayVisitors - lastWeekVisitors) / (decimal)lastWeekVisitors) * 100;
            }

            return Json(new
            {
                categories = dates,
                highData,
                lowData,
                todayVisitors,
                visitorsEfficiency
            });
        }


        [HttpGet]
        public async Task<IActionResult> GetBarChartData()
        {
            var monthNow = DateTime.Now.Month; // Current month
            var startMonth = Math.Max(1, monthNow - 3); // Ensure we don't go below January
            var currentYear = DateTime.Now.Year; // 2025
            var salesData = await _context.Orders
                .Where(o => o.SaleDate.Month >= startMonth && o.SaleDate.Month <= monthNow && o.IsPaid == true)
                .GroupBy(o => new { o.SaleDate.Month })
                .Select(g => new
                {
                    Month = g.Key.Month,
                    TotalRevenue = g.Sum(o => o.TotalAmount)
                })
                .OrderBy(g => g.Month)
                .ToListAsync();

            var purchaseData = await _context.Purchases
                .Where(p => p.PurchasedDate.Month >= startMonth && p.PurchasedDate.Month <= monthNow)
                .GroupBy(p => new { p.PurchasedDate.Month })
                .Select(g => new
                {
                    Month = g.Key.Month,
                    TotalCost = g.Sum(p => p.TotalAmount)
                })
                .OrderBy(g => g.Month)
                .ToListAsync();

            
            var months = Enumerable.Range(startMonth, monthNow).Select(m => $"Tháng {m}").ToArray();
            var revenue = months.Select(m => salesData.FirstOrDefault(s => s.Month == int.Parse(m.Replace("Tháng ", "")))?.TotalRevenue ?? 0).ToArray();
            var cost = months.Select(m => purchaseData.FirstOrDefault(p => p.Month == int.Parse(m.Replace("Tháng ", "")))?.TotalCost ?? 0).ToArray();
            var profit = months.Select(m => revenue[Array.IndexOf(months, m)] - cost[Array.IndexOf(months, m)]).ToArray();

            var todayRevenue = _context.Orders
                .Where(o => o.SaleDate.Date == DateTime.Now.Date && o.IsPaid == true)
                .Sum(o => o.TotalAmount)
                .ToString("C", new System.Globalization.CultureInfo("vi-VN"));

            var today = DateTime.Today;
            var firstDayOfThisMonth = new DateTime(today.Year, today.Month, 1);
            var firstDayOfLastMonth = firstDayOfThisMonth.AddMonths(-1);

            var revenueThisMonth = _context.Orders
                .Where(o => o.IsPaid == true && o.SaleDate.Date >= firstDayOfThisMonth)
                .Sum(o => (decimal?)o.TotalAmount) ?? 0;

            var revenueLastMonth = _context.Orders
                .Where(o => o.IsPaid == true
                    && o.SaleDate.Date >= firstDayOfLastMonth
                    && o.SaleDate.Date < firstDayOfThisMonth)
                .Sum(o => (decimal?)o.TotalAmount) ?? 0;

            decimal efficiencyRaw;

            if (revenueLastMonth == 0)
            {
                efficiencyRaw = revenueThisMonth == 0 ? 0 : 100;
            }
            else
            {
                efficiencyRaw = ((revenueThisMonth - revenueLastMonth) / revenueLastMonth) * 100;
            }


            var efficiency = efficiencyRaw.ToString("F2", new System.Globalization.CultureInfo("vi-VN")) + "%";

            return Json(new { categories = months, revenue, cost, profit, todayRevenue, efficiency });
        }


       

    }
}
