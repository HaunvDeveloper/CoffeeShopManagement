using System;
using System.Collections.Generic;

namespace CoffeeManagement.Models;

public partial class ExportBill
{
    public long Id { get; set; }

    public long OrderId { get; set; }

    public DateTime ExportDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
