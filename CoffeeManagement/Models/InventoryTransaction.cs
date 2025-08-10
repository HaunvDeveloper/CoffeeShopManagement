using System;
using System.Collections.Generic;

namespace CoffeeManagement.Models;

public partial class InventoryTransaction
{
    public long Id { get; set; }

    public long InventoryId { get; set; }

    public decimal Quantity { get; set; }

    public string TransactionType { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public long CreatedByUserId { get; set; }

    public virtual User CreatedByUser { get; set; } = null!;

    public virtual Inventory Inventory { get; set; } = null!;
}
