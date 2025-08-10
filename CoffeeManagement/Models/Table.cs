using System;
using System.Collections.Generic;

namespace CoffeeManagement.Models;

public partial class Table
{
    public long Id { get; set; }

    public string TableNo { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public int Capacity { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
