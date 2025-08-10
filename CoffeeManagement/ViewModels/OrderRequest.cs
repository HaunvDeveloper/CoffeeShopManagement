using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeManagement.ViewModels
{
    public class OrderRequest
    {
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        public long CustomerId { get; set; }
        public long TableId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public bool IsTakeHome { get; set; } = false;
    }

    public class OrderItem
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Discount { get; set; }

        [NotMapped]
        public decimal TotalPrice => Price * Quantity * (1 - Discount / 100);
    }
}
