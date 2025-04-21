using LibraryManagementSystem.DTOs.Book;

namespace LibraryManagementSystem.Services.Interfaces
{
    public interface IBookService
    {
        Task<BookDTO> GetBookById(int id);
        Task<IList<BookDTO>> GetAllBooks();
        Task<bool> AddBook(BookRequestDTO BookDTO);
        Task<bool> UpdateBook(int id, BookRequestDTO BookDTO);
        Task<bool> DeleteBook(int id);
    }
}
