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
        Task<IList<BookDTO>> AddBooks(List<BookRequestDTO> bookDTOs);
        Task<IList<BookDTO>> GetAllBooksWithCopies();
        Task<BookDTO> GetBookWithCopies(int id);

        Task<BookCopyResponseDTO> GetBookCopyById(int id);
      
        Task<bool> AddBookCopy(BookCopyRequestDTO bookCopyDTO);
        Task<bool> UpdateBookCopy(int bookCopyId, BookCopyRequestDTO bookCopyDto);
        Task<bool> DeleteBookCopy(int bookCopyId);
    }
}
