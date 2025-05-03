using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Repositories.Interfaces
{
    public interface IBookRepository : IGenericRepository<Book>
    {
        Task<List<Book>> AddBooks(List<Book> books);

        Task<List<Book>> GetAllBooksWithCopies();
        Task<Book?> GetBookWithCopies(int id);
        Task<BookCopy?> GetBookCopyById(int id, bool getRelatedBook = true);
        Task<bool> AddBookCopy(BookCopy bookCopy, Book book);
        Task<bool> UpdateBookCopy(BookCopy bookCopy, Book book);
        Task<bool> DeleteBookCopy(BookCopy bookCopy, Book book);
    }
}
