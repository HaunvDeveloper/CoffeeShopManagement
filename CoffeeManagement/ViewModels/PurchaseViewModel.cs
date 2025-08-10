using CoffeeManagement.Models;

namespace CoffeeManagement.ViewModels
{
    public class PurchaseViewModel
    {
        public PurchaseViewModel() { }

        public PurchaseViewModel(Purchase purchase)
        {
            Id = purchase.Id;
            PurchasedNo = purchase.PurchasedNo;
            BillNo = purchase.BillNo;
            PurchasedDate = purchase.PurchasedDate;
            PurchasedDateStr = purchase.PurchasedDate.ToString("dd/MM/yyyy");
            PaymentDate = purchase.PaymentDate;
            PaymentDateStr = purchase.PaymentDate.ToString("dd/MM/yyyy");
            SupplierId = purchase.SupplierId;
            SupplierName = purchase.Supplier?.Name;
            EmployeeSaleId = purchase.EmployeeSaleId;
            EmployeeName = purchase.EmployeeSale?.FullName ?? string.Empty;
            CreatedUserId = purchase.CreatedUserId;
            TotalAmount = purchase.TotalAmount;
            TotalAmountStr = purchase.TotalAmount?.ToString("N0");
            PaymentStatus = purchase.PaymentStatus;
            Description = purchase.Description;
        }


        public long Id { get; set; }

        public string PurchasedNo { get; set; } = null!;

        public string? BillNo { get; set; }

        public DateTime PurchasedDate { get; set; }

        public string PurchasedDateStr { get; set; } = string.Empty;

        public DateTime PaymentDate { get; set; }

        public string PaymentDateStr { get; set; } = string.Empty;

        public long? SupplierId { get; set; }

        public string? SupplierName { get; set; }

        public long? EmployeeSaleId { get; set; }

        public string EmployeeName { get; set; } = string.Empty;

        public long? CreatedUserId { get; set; }

        public decimal? TotalAmount { get; set; }

        public string? TotalAmountStr { get; set; }

        public string PaymentStatus { get; set; } = null!;

        public string? Description { get; set; }
    }
}
