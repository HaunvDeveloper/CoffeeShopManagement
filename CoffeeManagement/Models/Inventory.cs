using System;
using System.Collections.Generic;

namespace CoffeeManagement.Models;

public partial class Inventory
{
    public long Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public decimal Quantity { get; set; }

    public string? Unit { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();

    public virtual ICollection<PurchaseDetail> PurchaseDetails { get; set; } = new List<PurchaseDetail>();
}
