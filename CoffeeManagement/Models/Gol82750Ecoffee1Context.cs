using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CoffeeManagement.Models;

public partial class Gol82750Ecoffee1Context : DbContext
{
    public Gol82750Ecoffee1Context()
    {
    }

    public Gol82750Ecoffee1Context(DbContextOptions<Gol82750Ecoffee1Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<EmployeePosition> EmployeePositions { get; set; }

    public virtual DbSet<EmployeeStatus> EmployeeStatuses { get; set; }

    public virtual DbSet<ExportBill> ExportBills { get; set; }

    public virtual DbSet<Inventory> Inventories { get; set; }

    public virtual DbSet<InventoryTransaction> InventoryTransactions { get; set; }

    public virtual DbSet<LeaveRequest> LeaveRequests { get; set; }

    public virtual DbSet<Menu> Menus { get; set; }

    public virtual DbSet<MenuItem> MenuItems { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<PaySalary> PaySalaries { get; set; }

    public virtual DbSet<PaySalaryDetail> PaySalaryDetails { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Purchase> Purchases { get; set; }

    public virtual DbSet<PurchaseDetail> PurchaseDetails { get; set; }

    public virtual DbSet<SaleStatus> SaleStatuses { get; set; }

    public virtual DbSet<Shift> Shifts { get; set; }

    public virtual DbSet<ShiftConfirmation> ShiftConfirmations { get; set; }

    public virtual DbSet<ShiftRegistration> ShiftRegistrations { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<Table> Tables { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserGroup> UserGroups { get; set; }

    public virtual DbSet<WorkSchedule> WorkSchedules { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("gol82750_Ecoffee1");

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Category", "dbo");

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customer", "dbo");

            entity.Property(e => e.Address).HasMaxLength(512);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CustomerNo).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.PhoneNo).HasMaxLength(50);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("Employee", "dbo");

            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FiredDate).HasColumnType("datetime");
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Gender)
                .HasMaxLength(10)
                .HasDefaultValue("Nam");
            entity.Property(e => e.HiredDate).HasColumnType("datetime");
            entity.Property(e => e.PhoneNo).HasMaxLength(20);
            entity.Property(e => e.ResignedDate).HasColumnType("datetime");
            entity.Property(e => e.SalaryPerHour).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Position).WithMany(p => p.Employees)
                .HasForeignKey(d => d.PositionId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Employee_EmployeePosition");

            entity.HasOne(d => d.Status).WithMany(p => p.Employees)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Employee_EmployeeStatus");

            entity.HasOne(d => d.User).WithMany(p => p.Employees)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Employee_User");
        });

        modelBuilder.Entity<EmployeePosition>(entity =>
        {
            entity.ToTable("EmployeePosition", "dbo");

            entity.Property(e => e.Description).HasMaxLength(215);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.SalaryPerHour).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<EmployeeStatus>(entity =>
        {
            entity.ToTable("EmployeeStatus", "dbo");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<ExportBill>(entity =>
        {
            entity.ToTable("ExportBill", "dbo");

            entity.Property(e => e.ExportDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Success");

            entity.HasOne(d => d.Order).WithMany(p => p.ExportBills)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_ExportBill_Order");
        });

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.ToTable("Inventory", "dbo");

            entity.Property(e => e.Code).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Unit).HasMaxLength(20);
        });

        modelBuilder.Entity<InventoryTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Inventor__3214EC07A241EBC8");

            entity.ToTable("InventoryTransaction", "dbo");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TransactionType).HasMaxLength(20);

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.InventoryTransactions)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InventoryTransaction_User");

            entity.HasOne(d => d.Inventory).WithMany(p => p.InventoryTransactions)
                .HasForeignKey(d => d.InventoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InventoryTransaction_Inventory");
        });

        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LeaveReq__3214EC0770F2002B");

            entity.ToTable("LeaveRequest", "dbo");

            entity.Property(e => e.ApprovedAt).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.ApprovedByUser).WithMany(p => p.LeaveRequests)
                .HasForeignKey(d => d.ApprovedByUserId)
                .HasConstraintName("FK_LeaveRequest_User");

            entity.HasOne(d => d.Employee).WithMany(p => p.LeaveRequests)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LeaveRequest_Employee");
        });

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Menu__3214EC074749BB04");

            entity.ToTable("Menu", "dbo");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.Menus)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Menu_User");
        });

        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MenuItem__3214EC078CC6BFD4");

            entity.ToTable("MenuItem", "dbo");

            entity.HasOne(d => d.Menu).WithMany(p => p.MenuItems)
                .HasForeignKey(d => d.MenuId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MenuItem_Menu");

            entity.HasOne(d => d.Product).WithMany(p => p.MenuItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MenuItem_Product");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Sale");

            entity.ToTable("Order", "dbo");

            entity.Property(e => e.BillNo).HasMaxLength(50);
            entity.Property(e => e.CustomerName).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsUsing).HasDefaultValue(true);
            entity.Property(e => e.LeaveTime).HasColumnType("datetime");
            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.SaleDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SaleNo).HasMaxLength(50);
            entity.Property(e => e.SaleStatusId).HasDefaultValue(1L);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.CreatedUser).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CreatedUserId)
                .HasConstraintName("FK_Sale_User");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_Sale_Customer");

            entity.HasOne(d => d.EmployeeSale).WithMany(p => p.Orders)
                .HasForeignKey(d => d.EmployeeSaleId)
                .HasConstraintName("FK_Sale_Employee");

            entity.HasOne(d => d.SaleStatus).WithMany(p => p.Orders)
                .HasForeignKey(d => d.SaleStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sale_SaleStatus");

            entity.HasOne(d => d.Table).WithMany(p => p.Orders)
                .HasForeignKey(d => d.TableId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Sale_Table");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_SaleDetail");

            entity.ToTable("OrderDetail", "dbo");

            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.Discount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ProductName).HasMaxLength(100);
            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SaleDetail_Product");

            entity.HasOne(d => d.Sale).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.SaleId)
                .HasConstraintName("FK_SaleDetail_Sale");
        });

        modelBuilder.Entity<PaySalary>(entity =>
        {
            entity.ToTable("PaySalary", "dbo");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedByUsername)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Month).HasDefaultValue(1);
            entity.Property(e => e.PayDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TotalPay).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Year).HasDefaultValue(2025);
        });

        modelBuilder.Entity<PaySalaryDetail>(entity =>
        {
            entity.ToTable("PaySalaryDetail", "dbo");

            entity.Property(e => e.BaseSalary).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Bonus).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Deduction).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.EmployeeName).HasMaxLength(100);
            entity.Property(e => e.OvertimeSalary).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Total).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Employee).WithMany(p => p.PaySalaryDetails)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_PaySalaryDetail_Employee");

            entity.HasOne(d => d.PaySalary).WithMany(p => p.PaySalaryDetails)
                .HasForeignKey(d => d.PaySalaryId)
                .HasConstraintName("FK_PaySalaryDetail_PaySalary");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("Permission", "dbo");

            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Module).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Product", "dbo");

            entity.Property(e => e.Code).HasMaxLength(100);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Origin).HasMaxLength(50);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Unit).HasMaxLength(50);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            entity.Property(e => e.Vat).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Product_Category");
        });

        modelBuilder.Entity<Purchase>(entity =>
        {
            entity.ToTable("Purchase", "dbo");

            entity.Property(e => e.BillNo).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.PaymentDate).HasColumnType("datetime");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(50)
                .HasDefaultValue("CHƯA");
            entity.Property(e => e.PurchasedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PurchasedNo).HasMaxLength(50);
            entity.Property(e => e.SupplierName).HasMaxLength(200);
            entity.Property(e => e.TotalAmount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.CreatedUser).WithMany(p => p.Purchases)
                .HasForeignKey(d => d.CreatedUserId)
                .HasConstraintName("FK_Purchase_User");

            entity.HasOne(d => d.EmployeeSale).WithMany(p => p.Purchases)
                .HasForeignKey(d => d.EmployeeSaleId)
                .HasConstraintName("FK_Purchase_Employee");

            entity.HasOne(d => d.Supplier).WithMany(p => p.Purchases)
                .HasForeignKey(d => d.SupplierId)
                .HasConstraintName("FK_Purchase_Supplier");
        });

        modelBuilder.Entity<PurchaseDetail>(entity =>
        {
            entity.ToTable("PurchaseDetail", "dbo");

            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.ItemName).HasMaxLength(100);
            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Vat)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("VAT");

            entity.HasOne(d => d.Item).WithMany(p => p.PurchaseDetails)
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("FK_PurchaseDetail_Inventory");

            entity.HasOne(d => d.Purchase).WithMany(p => p.PurchaseDetails)
                .HasForeignKey(d => d.PurchaseId)
                .HasConstraintName("FK_PurchaseDetail_Purchase");
        });

        modelBuilder.Entity<SaleStatus>(entity =>
        {
            entity.ToTable("SaleStatus", "dbo");

            entity.Property(e => e.Code).HasMaxLength(20);
            entity.Property(e => e.Description).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Shift>(entity =>
        {
            entity.ToTable("Shift", "dbo");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<ShiftConfirmation>(entity =>
        {
            entity.ToTable("ShiftConfirmation", "dbo");

            entity.Property(e => e.ApprovedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ApprovedbyUsername).HasMaxLength(50);
            entity.Property(e => e.RejectReason).HasMaxLength(200);

            entity.HasOne(d => d.ApprovedByUser).WithMany(p => p.ShiftConfirmations)
                .HasForeignKey(d => d.ApprovedByUserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_ShiftConfirmation_User");

            entity.HasOne(d => d.Registration).WithMany(p => p.ShiftConfirmations)
                .HasForeignKey(d => d.RegistrationId)
                .HasConstraintName("FK_ShiftConfirmation_ShiftRegistration");
        });

        modelBuilder.Entity<ShiftRegistration>(entity =>
        {
            entity.ToTable("ShiftRegistration", "dbo");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Employee).WithMany(p => p.ShiftRegistrations)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK_EmployeeShift_Employee");

            entity.HasOne(d => d.Shift).WithMany(p => p.ShiftRegistrations)
                .HasForeignKey(d => d.ShiftId)
                .HasConstraintName("FK_EmployeeShift_Shift");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.ToTable("Supplier", "dbo");

            entity.Property(e => e.Address).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(512);
            entity.Property(e => e.PhoneNo).HasMaxLength(50);
            entity.Property(e => e.ShortName).HasMaxLength(100);
            entity.Property(e => e.SupplierNo).HasMaxLength(50);
        });

        modelBuilder.Entity<Table>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Table__3214EC0743703DFB");

            entity.ToTable("Table", "dbo");

            entity.Property(e => e.Capacity).HasDefaultValue(1);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Available");
            entity.Property(e => e.TableNo).HasMaxLength(20);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User", "dbo");

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.UserGroup).WithMany(p => p.Users)
                .HasForeignKey(d => d.UserGroupId)
                .HasConstraintName("FK_User_UserGroup");
        });

        modelBuilder.Entity<UserGroup>(entity =>
        {
            entity.ToTable("UserGroup", "dbo");

            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Name).HasMaxLength(50);

            entity.HasMany(d => d.Permissions).WithMany(p => p.UserGroups)
                .UsingEntity<Dictionary<string, object>>(
                    "UserGroupPermission",
                    r => r.HasOne<Permission>().WithMany()
                        .HasForeignKey("PermissionId")
                        .HasConstraintName("FK_UserGroupPermission_Permission"),
                    l => l.HasOne<UserGroup>().WithMany()
                        .HasForeignKey("UserGroupId")
                        .HasConstraintName("FK_UserGroupPermission_UserGroup"),
                    j =>
                    {
                        j.HasKey("UserGroupId", "PermissionId");
                        j.ToTable("UserGroupPermission", "dbo");
                    });
        });

        modelBuilder.Entity<WorkSchedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__WorkSche__3214EC07E3440FA3");

            entity.ToTable("WorkSchedule", "dbo");

            entity.Property(e => e.CheckinTime).HasColumnType("datetime");
            entity.Property(e => e.CheckoutTime).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("CHƯA HOÀN THÀNH");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.WorkSchedules)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WorkSchedule_User");

            entity.HasOne(d => d.Employee).WithMany(p => p.WorkSchedules)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WorkSchedule_Employee");

            entity.HasOne(d => d.Shift).WithMany(p => p.WorkSchedules)
                .HasForeignKey(d => d.ShiftId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WorkSchedule_Shift");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
