using System;
using System.Collections.Generic;

namespace CoffeeManagement.Models;

public partial class WorkSchedule
{
    public long Id { get; set; }

    public long EmployeeId { get; set; }

    public long ShiftId { get; set; }

    public DateOnly WorkDate { get; set; }

    public bool IsDone { get; set; }

    public bool IsLate { get; set; }

    public TimeOnly? LateTime { get; set; }

    public DateTime? CheckinTime { get; set; }

    public DateTime? CheckoutTime { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public long CreatedByUserId { get; set; }

    public virtual User CreatedByUser { get; set; } = null!;

    public virtual Employee Employee { get; set; } = null!;

    public virtual Shift Shift { get; set; } = null!;
}
