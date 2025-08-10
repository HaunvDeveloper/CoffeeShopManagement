using System;
using System.Collections.Generic;

namespace CoffeeManagement.ViewModels
{
    public class ProductViewModel
    {
        public long Id { get; set; }

        public string Code { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string? Unit { get; set; }

        public decimal Price { get; set; }

        public decimal Vat { get; set; }

        public string ImageBinary { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int StockQuantity { get; set; }

        public long? CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public string? Origin { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CreatedDateStr { get; set; } = string.Empty;

        public DateTime? UpdatedDate { get; set; }

        public string UpdatedDateStr { get; set; } = string.Empty;
    }
}