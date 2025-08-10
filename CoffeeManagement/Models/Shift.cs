using System;
using System.Collections.Generic;

namespace CoffeeManagement.Models;

public partial class Shift
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public virtual ICollection<ShiftRegistration> ShiftRegistrations { get; set; } = new List<ShiftRegistration>();

    public virtual ICollection<WorkSchedule> WorkSchedules { get; set; } = new List<WorkSchedule>();
}
