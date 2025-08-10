using CoffeeManagement.Models;

namespace CoffeeManagement.Services
{
    public class CustomerService
    {
        private readonly Gol82750Ecoffee1Context _context;
        public CustomerService(Gol82750Ecoffee1Context context)
        {
            _context = context;
        }
        
        public string GenerateNewCustomerNo()
        {
            var lastItem = _context.Customers
                                      .OrderByDescending(p => p.CustomerNo)
                                      .Select(p => p.CustomerNo)
                                      .FirstOrDefault();

            if (string.IsNullOrEmpty(lastItem))
            {
                return "KH0000001";
            }

            string numericPart = lastItem.Substring(2);
            int number;
            if (!int.TryParse(numericPart, out number))
            {
                throw new Exception("CustomerNo không hợp lệ trong cơ sở dữ liệu.");
            }

            number++;
            return $"KH{number:D7}";
        }
    }
}
