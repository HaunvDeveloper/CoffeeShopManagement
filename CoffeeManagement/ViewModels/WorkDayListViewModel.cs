using CoffeeManagement.Models;

namespace CoffeeManagement.ViewModels
{
    public class WorkDayListViewModel
    {
        public long RegisterId {  get; set; }

        public long ShiftId { get; set; }

        public DateOnly WorkDate { get; set; }

        public Employee Employee { get; set; } = new Employee();

        public bool? IsApproved { get; set; }
    }
}
