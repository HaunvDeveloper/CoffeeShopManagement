using System;
using System.Collections.Generic;

namespace CoffeeManagement.Models;

public partial class Customer
{
    public long Id { get; set; }

    public string CustomerNo { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public string? PhoneNo { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public DateTime CreatedAt { get; set; }

    public long CreatedByUserId { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
