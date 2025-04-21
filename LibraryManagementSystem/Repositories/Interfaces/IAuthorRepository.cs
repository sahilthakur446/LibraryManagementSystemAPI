using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Repositories.Interfaces
{
    public interface IAuthorRepository : IGenericRepository<Author>
    {
        Task<List<Book>> GetBooksByAuthorId(int authorId);
        Task<bool> DeleteBooksByAuthorId(int authorId);
    }
}
