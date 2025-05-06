using LibraryManagementSystem.Services.Interfaces;
using LibraryManagementSystem.Settings;

namespace LibraryManagementSystem.Services.Implementations
{
    public class FineCalculator : IFineCalculator
    {
        public int CalculateFine(DateTime dueDate)
        {
            var today = DateTime.Today;
            int overdueDays = Math.Max((today - dueDate).Days, 0);
            return overdueDays * LibrarySettings.FinePerDay;
        }
    }
}
