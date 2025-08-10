using System;
using System.Collections.Generic;

namespace CoffeeManagement.Models;

public partial class SaleStatus
{
    public long Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
