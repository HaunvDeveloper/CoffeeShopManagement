using System;
using System.Collections.Generic;

namespace CoffeeManagement.Models;

public partial class PurchaseDetail
{
    public long Id { get; set; }

    public long ItemId { get; set; }

    public string? ItemName { get; set; }

    public long PurchaseId { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Vat { get; set; }

    public string? Description { get; set; }

    public virtual Inventory Item { get; set; } = null!;

    public virtual Purchase Purchase { get; set; } = null!;
}
