using System;
using System.Collections.Generic;

namespace CoffeeManagement.Models;

public partial class ShiftRegistration
{
    public long Id { get; set; }

    public long EmployeeId { get; set; }

    public long ShiftId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateOnly WorkDate { get; set; }

    public bool? IsApproved { get; set; }

    public bool IsForce { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual Shift Shift { get; set; } = null!;

    public virtual ICollection<ShiftConfirmation> ShiftConfirmations { get; set; } = new List<ShiftConfirmation>();
}
