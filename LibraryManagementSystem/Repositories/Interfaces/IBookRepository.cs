using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Repositories.Interfaces
{
    public interface IBookRepository : IGenericRepository<Book>
    {
        Task<List<Book>> AddBooks(List<Book> books);

        Task<List<Book>> GetAllBooksWithCopies();
        Task<List<Book>> GetBookAllCopies();
        Task<bool> AddBookCopy(Book book);
        Task<bool> UpdateBookCopy(Book book);
        Task<bool> DeleteBookCopy(Book book);
    }
}
