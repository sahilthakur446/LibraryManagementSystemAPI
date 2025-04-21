using LibraryManagementSystem.DTOs.Category;
using LibraryManagementSystem.DTOs.Book;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.DTOs.Author;

namespace LibraryManagementSystem.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<CategoryDTO> GetCategoryById(int id);
        Task<IEnumerable<CategoryDTO>> GetAllCategories();
        Task<bool> AddCategory(CategoryRequestDTO CategoryDTO);
        Task<bool> UpdateCategory(int id, CategoryRequestDTO CategoryDTO);
        Task<bool> DeleteCategory(int id);
        Task<List<BookDTO>> GetBooksByCategory(int id);
        Task<bool> DeleteBooksByCategory(int id);
    }
}
