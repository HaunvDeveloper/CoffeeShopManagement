using System;
using System.Collections.Generic;

namespace CoffeeManagement.Models;

public partial class OrderDetail
{
    public long Id { get; set; }

    public long ProductId { get; set; }

    public long SaleId { get; set; }

    public string? ProductName { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Discount { get; set; }

    public string? Description { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual Order Sale { get; set; } = null!;
}
