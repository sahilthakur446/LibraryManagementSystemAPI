using LibraryManagementSystem.DTOs.Author;
using LibraryManagementSystem.DTOs.Book;

namespace LibraryManagementSystem.Services.Interfaces
{
    public interface IAuthorService
    {
        Task<AuthorDTO> GetAuthorById(int id);
        Task<IEnumerable<AuthorDTO>> GetAllAuthors();
        Task<bool> AddAuthor(AuthorRequestDTO authorDTO);
        Task<bool> UpdateAuthor(int id, AuthorRequestDTO authorDTO);
        Task<bool> DeleteAuthor(int id);
        Task<List<BookDTO>> GetBooksByAuthor(int id);
        Task<bool> DeleteBooksByAuthor(int id);

    }
}
