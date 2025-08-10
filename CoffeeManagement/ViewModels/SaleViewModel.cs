using CoffeeManagement.Models;
using System;
using System.Collections.Generic;

namespace CoffeeManagement.ViewModels;

public class SaleViewModel
{
    public SaleViewModel() { }

    public SaleViewModel(Order purchase)
    {
        Id = purchase.Id;
        SaleNo = purchase.SaleNo;
        BillNo = purchase.BillNo;
        SaleNo = purchase.SaleNo;
        SaleDateStr = purchase.SaleDate.ToString("dd/MM/yyyy");
        PaymentDate = purchase.PaymentDate;
        PaymentDateStr = purchase.PaymentDate?.ToString("dd/MM/yyyy") ?? "";
        CustomerId = purchase.CustomerId;
        CustomerName = purchase.Customer?.LastName + " " + purchase.Customer?.FirstName;
        EmployeeSaleId = purchase.EmployeeSaleId;
        EmployeeSaleName = purchase.EmployeeSale?.FullName ?? string.Empty;
        CreatedUserId = purchase.CreatedUserId;
        TotalAmount = purchase.TotalAmount;
        TotalAmountStr = purchase.TotalAmount.ToString("N0");
        Description = purchase.Description;
        IsPaid = purchase.IsPaid;
    }


    public long Id { get; set; }

    public string SaleNo { get; set; } = null!;

    public DateTime SaleDate { get; set; }
    public string SaleDateStr { get; set; } = string.Empty;

    public DateTime? PaymentDate { get; set; }
    public string PaymentDateStr { get; set; } = string.Empty;

    public string? BillNo { get; set; }

    public long? CustomerId { get; set; }

    public string? CustomerName { get; set; }

    public long? EmployeeSaleId { get; set; }

    public string EmployeeSaleName { get; set; } = string.Empty;

    public long? CreatedUserId { get; set; }

    public decimal TotalAmount { get; set; }
    public string TotalAmountStr { get; set; } = string.Empty;

    public string PaymentStatus { get; set; } = null!;

    public long SaleStatusId { get; set; }

    public string SaleStatusName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsPaid { get; set; }

    public string IsPaidStr
    {
        get
        {
            return IsPaid ? "Đã thanh toán" : "Chưa thanh toán";
        }
    }

}
