using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Services.Interfaces
{
    public interface IFineService
    {
        public Task<IEnumerable<Fine>> GetAllFines();
        public Task<Fine?> GetFineById(int id);
        public Task<bool> InsertFine(Fine fine);
        public Task<bool> UpdateFine(int id, Fine fine);
        public Task<bool> DeleteFineById(int id);
    }
}
