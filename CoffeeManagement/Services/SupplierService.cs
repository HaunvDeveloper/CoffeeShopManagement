using CoffeeManagement.Models;

namespace CoffeeManagement.Services
{
    public class SupplierService
    {
        private readonly Gol82750Ecoffee1Context _context;

        public SupplierService(Gol82750Ecoffee1Context context)
        {
            _context = context;
        }

        public string GenerateNewSupplierNo()
        {
            var lastPurchaseNo = _context.Suppliers
                                      .OrderByDescending(p => p.SupplierNo)
                                      .Select(p => p.SupplierNo)
                                      .FirstOrDefault();

            if (string.IsNullOrEmpty(lastPurchaseNo))
            {
                return "NCC0000001"; 
            }

            string numericPart = lastPurchaseNo.Substring(3); 
            int number;
            if (!int.TryParse(numericPart, out number))
            {
                throw new Exception("SupplierNo không hợp lệ trong cơ sở dữ liệu.");
            }

            number++;
            return $"NCC{number:D7}";
        }

    }
}
