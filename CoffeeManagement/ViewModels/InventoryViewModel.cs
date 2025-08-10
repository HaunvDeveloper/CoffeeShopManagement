namespace CoffeeManagement.ViewModels
{ 
    public partial class InventoryViewModel
    {
        public long Id { get; set; }

        public string Code { get; set; } = null!;

        public string Name { get; set; } = null!;

        public decimal Quantity { get; set; }

        public string QuantityStr { get; set; } = string.Empty;

        public string? Unit { get; set; }

        public DateTime CreatedAt { get; set; }

    }

}
