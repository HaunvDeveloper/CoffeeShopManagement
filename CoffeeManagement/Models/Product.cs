using System;
using System.Collections.Generic;

namespace CoffeeManagement.Models;

public partial class Product
{
    public long Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Unit { get; set; }

    public decimal Price { get; set; }

    public decimal Vat { get; set; }

    public byte[]? ImageBinary { get; set; }

    public string? Description { get; set; }

    public int StockQuantity { get; set; }

    public long? CategoryId { get; set; }

    public string? Origin { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
