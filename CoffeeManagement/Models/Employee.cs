using System;
using System.Collections.Generic;

namespace CoffeeManagement.Models;

public partial class Employee
{
    public long Id { get; set; }

    public string FullName { get; set; } = null!;

    public string? PhoneNo { get; set; }

    public DateOnly? DayOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? Address { get; set; }

    public string? Email { get; set; }

    public DateTime HiredDate { get; set; }

    public long? StatusId { get; set; }

    public long? PositionId { get; set; }

    public bool HasAccount { get; set; }

    public long? UserId { get; set; }

    public DateTime? FiredDate { get; set; }

    public decimal SalaryPerHour { get; set; }

    public DateTime? ResignedDate { get; set; }

    public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<PaySalaryDetail> PaySalaryDetails { get; set; } = new List<PaySalaryDetail>();

    public virtual EmployeePosition? Position { get; set; }

    public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();

    public virtual ICollection<ShiftRegistration> ShiftRegistrations { get; set; } = new List<ShiftRegistration>();

    public virtual EmployeeStatus? Status { get; set; }

    public virtual User? User { get; set; }

    public virtual ICollection<WorkSchedule> WorkSchedules { get; set; } = new List<WorkSchedule>();
}
