using CoffeeManagement.Models;

namespace CoffeeManagement.ViewModels
{
    public class TableViewModel
    {
        public long Id { get; set; }

        public string TableNo { get; set; } = null!;

        public string Status { get; set; } = null!;

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public int Capacity { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal RemainingAmount { get; set; }

        public int TotalItem { get; set; }

        public bool IsPaid { get; set; }    

        public DateTime? FirstGetIn { get; set; }
    }
}
