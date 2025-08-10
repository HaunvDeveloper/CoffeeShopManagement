using System;
using System.Collections.Generic;

namespace CoffeeManagement.Models;

public partial class EmployeeStatus
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
