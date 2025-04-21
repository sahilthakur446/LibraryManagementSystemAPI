using LibraryManagementSystem.Exceptions;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LibraryManagementSystem.Repositories.EF
{
    public class CategoryRepositoryEF : ICategoryRepository
    {
        private readonly LibraryDbContext dbContext;
        private readonly ILogger<CategoryRepositoryEF> logger;

        public CategoryRepositoryEF(LibraryDbContext dbContext, ILogger<CategoryRepositoryEF> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                var category = await dbContext.Categories.FindAsync(id);
                if (category is null)
                {
                    logger.LogInformation("No category found with ID: {Id}", id);
                    return false;
                }
                dbContext.Categories.Remove(category);
                return await dbContext.SaveChangesAsync() > 0;
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Database update error while deleting category with ID: {Id}", id);
                throw new RepositoryException("Database error occurred while deleting category.", ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while deleting category with ID: {Id}", id);
                throw new RepositoryException("Unexpected error occurred while deleting category.", ex);
            }
        }

        public async Task<bool> DeleteBooksByCategoryId(int categoryId)
        {
            try
            {
                var booksToRemove = await dbContext.Books
                    .Where(b => b.CategoryId == categoryId)
                    .ToListAsync();

                if (!booksToRemove.Any())
                {
                    logger.LogInformation("No Books found for Category with ID: {Id}", categoryId);
                    return false;
                }

                dbContext.Books.RemoveRange(booksToRemove);
                return await dbContext.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while deleting books for Category with ID: {Id}", categoryId);
                throw new RepositoryException("Unexpected error occurred while deleting books.", ex);
            }
        }

        public async Task<IEnumerable<Category?>> GetAll()
        {
            try
            {
                return await dbContext.Categories.ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error occurred while fetching categories.");
                throw new RepositoryException("Unexpected error occurred while fetching categories.", ex);
            }
        }

        public async Task<List<Book>> GetBooksByCategoryId(int categoryId)
        {
            try
            {
                return await dbContext.Books
                    .Where(b => b.CategoryId == categoryId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while fetching books for Category ID: {Id}", categoryId);
                throw new RepositoryException("Unexpected error occurred while fetching books.", ex);
            }
        }

        public async Task<Category?> GetById(int id)
        {
            try
            {
                return await dbContext.Categories.FirstOrDefaultAsync(c => c.CategoryId == id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error occurred while fetching category with ID: {Id}", id);
                throw new RepositoryException("Unexpected error occurred while fetching category.", ex);
            }
        }

        public async Task<bool> Insert(Category category)
        {
            try
            {
                await dbContext.Categories.AddAsync(category);
                return await dbContext.SaveChangesAsync() > 0;
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Database update error while inserting category.");
                throw new RepositoryException("Database error occurred while inserting category.", ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error occurred while inserting category.");
                throw new RepositoryException("Unexpected error occurred while inserting category.", ex);
            }
        }

        public async Task<bool> Update(Category category)
        {
            try
            {
                dbContext.Categories.Update(category);
                return await dbContext.SaveChangesAsync() > 0;
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Database update error while updating category with ID: {Id}", category.CategoryId);
                throw new RepositoryException("Database error occurred while updating category.", ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error occurred while updating category with ID: {Id}", category.CategoryId);
                throw new RepositoryException("Unexpected error occurred while updating category.", ex);
            }
        }
    }
}
