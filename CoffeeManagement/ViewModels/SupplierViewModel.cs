namespace CoffeeManagement.ViewModels
{
    public class SupplierViewModel
    {
        public long Id { get; set; }

        public string SupplierNo { get; set; } = null!;

        public string? ShortName { get; set; }

        public string? Name { get; set; }

        public string? PhoneNo { get; set; }

        public string? Email { get; set; }

        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; }

        public long CreatedByUserId { get; set; }

    }

}
