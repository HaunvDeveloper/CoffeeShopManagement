using System;
using System.Collections.Generic;

namespace CoffeeManagement.Models;

public partial class Purchase
{
    public long Id { get; set; }

    public string PurchasedNo { get; set; } = null!;

    public string? BillNo { get; set; }

    public DateTime PurchasedDate { get; set; }

    public DateTime PaymentDate { get; set; }

    public long? SupplierId { get; set; }

    public string? SupplierName { get; set; }

    public long? EmployeeSaleId { get; set; }

    public long? CreatedUserId { get; set; }

    public decimal? TotalAmount { get; set; }

    public string PaymentStatus { get; set; } = null!;

    public string? Description { get; set; }

    public virtual User? CreatedUser { get; set; }

    public virtual Employee? EmployeeSale { get; set; }

    public virtual ICollection<PurchaseDetail> PurchaseDetails { get; set; } = new List<PurchaseDetail>();

    public virtual Supplier? Supplier { get; set; }
}
