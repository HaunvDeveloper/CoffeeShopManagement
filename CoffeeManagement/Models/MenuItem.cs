using System;
using System.Collections.Generic;

namespace CoffeeManagement.Models;

public partial class MenuItem
{
    public long Id { get; set; }

    public long MenuId { get; set; }

    public long ProductId { get; set; }

    public virtual Menu Menu { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
