using CoffeeManagement.Models;

namespace CoffeeManagement.Services
{
    public class OrderService
    {
        private readonly Gol82750Ecoffee1Context _context;

        public OrderService(Gol82750Ecoffee1Context context)
        {
            _context = context;
        }

        public string GenerateNewSaleNo()
        {
            var lastSaleNo = _context.Orders
                                      .OrderByDescending(p => p.SaleNo)
                                      .Select(p => p.SaleNo)
                                      .FirstOrDefault();

            if (string.IsNullOrEmpty(lastSaleNo))
            {
                return "BH0000001"; 
            }

            string numericPart = lastSaleNo.Substring(2); 
            int number;
            if (!int.TryParse(numericPart, out number))
            {
                throw new Exception("SaleNo không hợp lệ trong cơ sở dữ liệu.");
            }

            number++;
            return $"BH{number:D7}";
        }

        public string GenerateNewBillNo()
        {
            var today = DateTime.Today;

            // Lọc các hóa đơn có SaleDate là hôm nay
            var todaySales = _context.Orders
                .Where(s => s.SaleDate.Date == today)
                .Select(s => s.BillNo)
                .ToList();

            if (todaySales == null || !todaySales.Any())
            {
                return "HD0000001";
            }

            // Lấy phần số của các bill hiện có, loại bỏ tiền tố "HD"
            var maxNumber = todaySales
                .Select(bill => int.TryParse(bill?.Substring(2), out var num) ? num : 0)
                .Max();

            // Tăng số lên 1 và format lại
            var newNumber = maxNumber + 1;
            return $"HD{newNumber:D7}";
        }

    }
}
