using CoffeeManagement.Models;

namespace CoffeeManagement.ViewModels
{
    public class PurchaseDetailViewModel
    {
        public long Id { get; set; }

        public long ItemId { get; set; }

        public string? ItemName { get; set; }

        public long PurchaseId { get; set; }

        public decimal Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal? Vat { get; set; }

        public string? Description { get; set; }

    }
}
