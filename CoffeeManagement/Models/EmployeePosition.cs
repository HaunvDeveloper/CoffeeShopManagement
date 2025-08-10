using System;
using System.Collections.Generic;

namespace CoffeeManagement.Models;

public partial class EmployeePosition
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal SalaryPerHour { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
