using System;
using System.Collections.Generic;

namespace CoffeeManagement.Models;

public partial class Menu
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public long CreatedByUserId { get; set; }

    public virtual User CreatedByUser { get; set; } = null!;

    public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
}
