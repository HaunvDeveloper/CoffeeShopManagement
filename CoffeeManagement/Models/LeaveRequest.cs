using System;
using System.Collections.Generic;

namespace CoffeeManagement.Models;

public partial class LeaveRequest
{
    public long Id { get; set; }

    public long EmployeeId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Reason { get; set; } = null!;

    public string Status { get; set; } = null!;

    public long? ApprovedByUserId { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? ApprovedByUser { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
