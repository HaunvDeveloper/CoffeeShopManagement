using System;
using System.Collections.Generic;

namespace CoffeeManagement.Models;

public partial class User
{
    public long Id { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? FullName { get; set; }

    public DateOnly? DayOfBirth { get; set; }

    public string? Email { get; set; }

    public bool IsBlock { get; set; }

    public long UserGroupId { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();

    public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();

    public virtual ICollection<Menu> Menus { get; set; } = new List<Menu>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();

    public virtual ICollection<ShiftConfirmation> ShiftConfirmations { get; set; } = new List<ShiftConfirmation>();

    public virtual UserGroup UserGroup { get; set; } = null!;

    public virtual ICollection<WorkSchedule> WorkSchedules { get; set; } = new List<WorkSchedule>();
}
