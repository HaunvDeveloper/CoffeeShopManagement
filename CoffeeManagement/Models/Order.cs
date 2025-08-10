using System;
using System.Collections.Generic;

namespace CoffeeManagement.Models;

public partial class Order
{
    public long Id { get; set; }

    public string SaleNo { get; set; } = null!;

    public DateTime SaleDate { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? BillNo { get; set; }

    public long? CustomerId { get; set; }

    public string? CustomerName { get; set; }

    public long? EmployeeSaleId { get; set; }

    public long? CreatedUserId { get; set; }

    public decimal TotalAmount { get; set; }

    public bool IsTakeHome { get; set; }

    public bool IsPaid { get; set; }

    public string? PaymentMethod { get; set; }

    public long SaleStatusId { get; set; }

    public long? TableId { get; set; }

    public bool? IsUsing { get; set; }

    public DateTime? LeaveTime { get; set; }

    public string? Description { get; set; }

    public virtual User? CreatedUser { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual Employee? EmployeeSale { get; set; }

    public virtual ICollection<ExportBill> ExportBills { get; set; } = new List<ExportBill>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual SaleStatus SaleStatus { get; set; } = null!;

    public virtual Table? Table { get; set; }
}
