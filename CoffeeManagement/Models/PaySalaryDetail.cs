using System;
using System.Collections.Generic;

namespace CoffeeManagement.Models;

public partial class PaySalaryDetail
{
    public long Id { get; set; }

    public long PaySalaryId { get; set; }

    public long? EmployeeId { get; set; }

    public string? EmployeeName { get; set; }

    public decimal BaseSalary { get; set; }

    public decimal Bonus { get; set; }

    public decimal OvertimeSalary { get; set; }

    public decimal Deduction { get; set; }

    public decimal Total { get; set; }

    public string? Status { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual PaySalary PaySalary { get; set; } = null!;
}
