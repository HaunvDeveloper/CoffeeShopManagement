using System;
using System.Collections.Generic;

namespace CoffeeManagement.Models;

public partial class PaySalary
{
    public long Id { get; set; }

    public int Month { get; set; }

    public int Year { get; set; }

    public DateTime CreatedAt { get; set; }

    public decimal TotalPay { get; set; }

    public string? Description { get; set; }

    public long? CreatedByUserId { get; set; }

    public string? CreatedByUsername { get; set; }

    public string Status { get; set; } = null!;

    public DateTime PayDate { get; set; }

    public virtual ICollection<PaySalaryDetail> PaySalaryDetails { get; set; } = new List<PaySalaryDetail>();
}
