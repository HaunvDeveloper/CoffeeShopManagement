using CoffeeManagement.Models;

namespace CoffeeManagement.Services
{
    public class PurchaseService
    {
        private readonly Gol82750Ecoffee1Context _context;

        public PurchaseService(Gol82750Ecoffee1Context context)
        {
            _context = context;
        }

        public string GenerateNewPurchaseNo()
        {
            var lastPurchaseNo = _context.Purchases
                                      .OrderByDescending(p => p.PurchasedNo)
                                      .Select(p => p.PurchasedNo)
                                      .FirstOrDefault();

            if (string.IsNullOrEmpty(lastPurchaseNo))
            {
                return "MH0000001"; 
            }

            string numericPart = lastPurchaseNo.Substring(2); 
            int number;
            if (!int.TryParse(numericPart, out number))
            {
                throw new Exception("PurchaseNo không hợp lệ trong cơ sở dữ liệu.");
            }

            number++;
            return $"MH{number:D7}";
        }

    }
}
