using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Repositories.Interfaces
{
    public interface IFineRepository : IGenericRepository<Fine>
    {
        Task<Fine?> GetUnpaidFineByUser(int userId);
    }
}
