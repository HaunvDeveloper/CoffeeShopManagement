using System;
using System.Collections.Generic;

namespace CoffeeManagement.Models;

public partial class ShiftConfirmation
{
    public long Id { get; set; }

    public long RegistrationId { get; set; }

    public bool IsApproved { get; set; }

    public DateTime ApprovedAt { get; set; }

    public string? RejectReason { get; set; }

    public long? ApprovedByUserId { get; set; }

    public string? ApprovedbyUsername { get; set; }

    public virtual User? ApprovedByUser { get; set; }

    public virtual ShiftRegistration Registration { get; set; } = null!;
}
