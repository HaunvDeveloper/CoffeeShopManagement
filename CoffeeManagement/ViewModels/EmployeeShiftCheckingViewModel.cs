using CoffeeManagement.Models;

namespace CoffeeManagement.ViewModels
{
    public class EmployeeShiftCheckingViewModel
    {
        public long Id { get; set; }

        public string FullName { get; set; } = null!;

        public string? PhoneNo { get; set; }

        public DateOnly? DayOfBirth { get; set; }

        public string? Gender { get; set; }

        public bool IsWork { get; set; }

        public string PositionName { get; set; } = string.Empty;

        public bool IsRegistered { get; set; }

        public string StatusRegist { get; set; } = string.Empty;
    }
}
