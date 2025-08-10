using CoffeeManagement.Models;

namespace CoffeeManagement.ViewModels
{
    public class EmployeeViewModel
    {
        // Constructor mặc định
        public EmployeeViewModel() { }

        // Constructor nhận đối tượng Employee
        public EmployeeViewModel(Employee employee)
        {
            this.Id = employee.Id;
            this.FullName = employee.FullName;
            this.PhoneNo = employee.PhoneNo;
            this.DayOfBirth = employee.DayOfBirth;
            this.Address = employee.Address;
            this.Email = employee.Email;
            this.HiredDate = employee.HiredDate;
            this.StatusId = employee.StatusId;
            this.PositionId = employee.PositionId;
            this.HasAccount = employee.HasAccount;
            this.UserId = employee.UserId;
            this.FiredDate = employee.FiredDate;
            this.Gender = employee.Gender;
            this.SalaryPerHour = employee.SalaryPerHour;

            // Nếu có các thuộc tính navigation như Position và Status, lấy tên của chúng.
            this.PositionName = employee.Position?.Name ?? string.Empty;  // Nếu Position null, gán string.Empty
            this.StatusName = employee.Status?.Name ?? string.Empty;  // Nếu Status null, gán string.Empty
        }

        public long Id { get; set; }

        public string FullName { get; set; } = null!;

        public string? PhoneNo { get; set; }

        public DateOnly? DayOfBirth { get; set; }

        public string? Gender { get; set; }

        public string? Address { get; set; }

        public string? Email { get; set; }

        public DateTime HiredDate { get; set; }

        public long? StatusId { get; set; }

        public long? PositionId { get; set; }

        public bool HasAccount { get; set; }

        public long? UserId { get; set; }

        public DateTime? FiredDate { get; set; }

        public decimal SalaryPerHour { get; set; }


        //------extend------------

        // Các thuộc tính bổ sung cho ViewModel
        public string PositionName { get; set; } = string.Empty;

        public string StatusName { get; set; } = string.Empty;
    }
}
