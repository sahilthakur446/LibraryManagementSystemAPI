using LibraryManagementSystem.DTOs.Book;
using LibraryManagementSystem.DTOs.Category;
using LibraryManagementSystem.Exceptions;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories.Interfaces;
using LibraryManagementSystem.Services.Interfaces;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository CategoryRepository;
    private readonly ILogger<CategoryService> logger;

    public CategoryService(ICategoryRepository CategoryRepository, ILogger<CategoryService> logger)
    {
        this.CategoryRepository = CategoryRepository;
        this.logger = logger;
    }

    public async Task<bool> AddCategory(CategoryRequestDTO CategoryDTO)
    {
        try
        {
            if (CategoryDTO == null || string.IsNullOrWhiteSpace(CategoryDTO.CategoryName))
            {
                logger.LogWarning("AddCategory: Invalid Category name received.");
                throw new BusinessExceptions("Category name cannot be empty.", StatusCodes.Status400BadRequest);
            }

            var newCategory = new Category { CategoryName = CategoryDTO.CategoryName };
            logger.LogInformation("AddCategory: Adding new Category '{CategoryName}'", newCategory.CategoryName);
            return await CategoryRepository.Insert(newCategory);
        }
        catch (RepositoryException ex)
        {
            logger.LogError(ex, "AddCategory: Repository error while adding Category '{CategoryName}'", CategoryDTO?.CategoryName);
            throw new BusinessExceptions("Failed to add Category due to internal error.", StatusCodes.Status500InternalServerError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AddCategory: Unexpected error while adding Category '{CategoryName}'", CategoryDTO?.CategoryName);
            throw new BusinessExceptions("An unexpected error occurred while adding Category.", StatusCodes.Status500InternalServerError, ex);
        }
    }

    public async Task<bool> DeleteCategory(int id)
    {
        if (id <= 0)
        {
            logger.LogWarning("DeleteCategory: Invalid ID {Id} provided.", id);
            throw new BusinessExceptions("Invalid Category ID provided.", StatusCodes.Status400BadRequest);
        }

        try
        {
            logger.LogInformation("DeleteCategory: Attempting to delete Category with ID {Id}", id);
            var result = await CategoryRepository.Delete(id);
            if (!result)
            {
                logger.LogWarning("DeleteCategory: Category with ID {Id} not found or already deleted.", id);
                throw new BusinessExceptions("Category not found or already deleted.", StatusCodes.Status404NotFound);
            }

            logger.LogInformation("DeleteCategory: Successfully deleted Category with ID {Id}", id);
            return result;
        }
        catch (RepositoryException ex)
        {
            logger.LogError(ex, "DeleteCategory: Repository error while deleting Category with ID {Id}", id);
            throw new BusinessExceptions("Failed to delete Category due to internal error.", StatusCodes.Status500InternalServerError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "DeleteCategory: Unexpected error while deleting Category with ID {Id}", id);
            throw new BusinessExceptions("An unexpected error occurred while deleting Category.", StatusCodes.Status500InternalServerError, ex);
        }
    }

    public async Task<IEnumerable<CategoryDTO>> GetAllCategories()
    {
        try
        {
            logger.LogInformation("GetAllCategories: Fetching all categories.");
            var categories = await CategoryRepository.GetAll();
            return categories.Select(c => new CategoryDTO { CategoryId = c.CategoryId, CategoryName = c.CategoryName });
        }
        catch (RepositoryException ex)
        {
            logger.LogError(ex, "GetAllCategories: Repository error while fetching categories.");
            throw new BusinessExceptions("Failed to fetch categories due to internal error.", StatusCodes.Status500InternalServerError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetAllCategories: Unexpected error while fetching categories.");
            throw new BusinessExceptions("An unexpected error occurred while retrieving categories.", StatusCodes.Status500InternalServerError, ex);
        }
    }

    public async Task<CategoryDTO?> GetCategoryById(int id)
    {
        if (id <= 0)
        {
            logger.LogWarning("GetCategoryById: Invalid Category ID {Id} provided.", id);
            throw new BusinessExceptions("Invalid Category ID.", StatusCodes.Status400BadRequest);
        }

        try
        {
            logger.LogInformation("GetCategoryById: Fetching Category with ID {Id}", id);
            var category = await CategoryRepository.GetById(id);
            if (category == null)
            {
                logger.LogWarning("GetCategoryById: Category with ID {Id} not found.", id);
                throw new BusinessExceptions("Category not found.", StatusCodes.Status404NotFound);
            }

            return new CategoryDTO { CategoryId = category.CategoryId, CategoryName = category.CategoryName };
        }
        catch (RepositoryException ex)
        {
            logger.LogError(ex, "GetCategoryById: Repository error while fetching Category with ID {Id}", id);
            throw new BusinessExceptions("Failed to retrieve Category due to internal error.", StatusCodes.Status500InternalServerError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetCategoryById: Unexpected error while fetching Category with ID {Id}", id);
            throw new BusinessExceptions("An unexpected error occurred while retrieving Category.", StatusCodes.Status500InternalServerError, ex);
        }
    }

    public async Task<List<BookDTO>> GetBooksByCategory(int id)
    {
        if (id <= 0)
        {
            logger.LogWarning("GetBooksByCategory: Invalid Category ID {Id}", id);
            throw new BusinessExceptions("Invalid Category ID.", StatusCodes.Status400BadRequest);
        }

        try
        {
            logger.LogInformation("GetBooksByCategory: Fetching books for Category ID {Id}", id);
            var books = await CategoryRepository.GetBooksByCategoryId(id);
            if (books == null || !books.Any())
            {
                logger.LogWarning("GetBooksByCategory: No books found for Category ID {Id}", id);
                throw new BusinessExceptions("No books found in the selected Category.", StatusCodes.Status404NotFound);
            }

            return books.Select(b => new BookDTO { BookId = b.BookId, Title = b.Title }).ToList();
        }
        catch (RepositoryException ex)
        {
            logger.LogError(ex, "GetBooksByCategory: Repository error while fetching books for Category ID {Id}", id);
            throw new BusinessExceptions("Failed to fetch books due to internal error.", StatusCodes.Status500InternalServerError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetBooksByCategory: Unexpected error while fetching books for Category ID {Id}", id);
            throw new BusinessExceptions("An unexpected error occurred while retrieving books.", StatusCodes.Status500InternalServerError, ex);
        }
    }

    public async Task<bool> UpdateCategory(int id, CategoryRequestDTO CategoryDTO)
    {
        if (id <= 0 || CategoryDTO == null || string.IsNullOrWhiteSpace(CategoryDTO.CategoryName))
        {
            logger.LogWarning("UpdateCategory: Invalid data. ID: {Id}, Name: {CategoryName}", id, CategoryDTO?.CategoryName);
            throw new BusinessExceptions("Invalid input data for updating Category.", StatusCodes.Status400BadRequest);
        }

        try
        {
            logger.LogInformation("UpdateCategory: Attempting to update Category with ID {Id}", id);
            var category = await CategoryRepository.GetById(id);
            if (category == null)
            {
                logger.LogWarning("UpdateCategory: Category with ID {Id} not found.", id);
                throw new BusinessExceptions("Category not found.", StatusCodes.Status404NotFound);
            }

            category.CategoryName = CategoryDTO.CategoryName;
            return await CategoryRepository.Update(category);
        }
        catch (RepositoryException ex)
        {
            logger.LogError(ex, "UpdateCategory: Repository error while updating Category with ID {Id}", id);
            throw new BusinessExceptions("Failed to update Category due to internal error.", StatusCodes.Status500InternalServerError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "UpdateCategory: Unexpected error while updating Category with ID {Id}", id);
            throw new BusinessExceptions("An unexpected error occurred while updating Category.", StatusCodes.Status500InternalServerError, ex);
        }
    }

    public async Task<bool> DeleteBooksByCategory(int id)
    {
        if (id <= 0)
        {
            logger.LogWarning("DeleteBooksByCategory: Invalid Category ID {Id} provided.", id);
            throw new BusinessExceptions("Invalid Category ID. Please provide a valid positive ID.", StatusCodes.Status400BadRequest);
        }

        try
        {
            logger.LogInformation("DeleteBooksByCategory: Attempting to delete all books for Category ID {Id}.", id);

            var result = await CategoryRepository.DeleteBooksByCategoryId(id);

            if (!result)
            {
                logger.LogWarning("DeleteBooksByCategory: No books found or already deleted for Category ID {Id}.", id);
                throw new BusinessExceptions("No books found under the specified Category or they are already deleted.", StatusCodes.Status404NotFound);
            }

            logger.LogInformation("DeleteBooksByCategory: Successfully deleted all books for Category ID {Id}.", id);
            return true;
        }
        catch (RepositoryException ex)
        {
            logger.LogError(ex, "DeleteBooksByCategory: Repository error while deleting books for Category ID {Id}.", id);
            throw new BusinessExceptions("A repository error occurred while deleting books from the category.", StatusCodes.Status500InternalServerError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "DeleteBooksByCategory: Unexpected error occurred while deleting books for Category ID {Id}.", id);
            throw new BusinessExceptions("An unexpected error occurred while attempting to delete books from the category.", StatusCodes.Status500InternalServerError, ex);
        }
    }
}
