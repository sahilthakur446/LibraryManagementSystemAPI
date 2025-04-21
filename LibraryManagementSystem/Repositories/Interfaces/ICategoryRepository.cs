using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Repositories.Interfaces
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<List<Book>> GetBooksByCategoryId(int categoryId);
        Task<bool> DeleteBooksByCategoryId(int categoryId);
    }
}
