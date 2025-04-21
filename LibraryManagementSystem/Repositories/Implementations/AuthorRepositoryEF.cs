using LibraryManagementSystem.Exceptions;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Repositories.EF
{
    public class AuthorRepositoryEF : IAuthorRepository
    {
        private readonly LibraryDbContext dbContext;
        private readonly ILogger<AuthorRepositoryEF> logger;

        public AuthorRepositoryEF(LibraryDbContext dbContext, ILogger<AuthorRepositoryEF> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                var author = await dbContext.Authors.FindAsync(id);
                if (author is null)
                {
                    logger.LogInformation("No Author found with ID: {Id}", id);
                    return false;
                }

                // Ensure no books are associated before deleting the author
                var booksExist = await dbContext.Books.AnyAsync(b => b.AuthorId == id);
                if (booksExist)
                {
                    logger.LogWarning("Cannot delete Author ID: {Id} as there are associated books.", id);
                    return false;
                }

                dbContext.Authors.Remove(author);
                return await dbContext.SaveChangesAsync() > 0;
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Database update error while deleting Author with ID: {Id}", id);
                throw new RepositoryException("Database error occurred while deleting Author.", ex);
            }
        }

        public async Task<bool> DeleteBooksByAuthorId(int authorId)
        {
            try
            {
                var booksToRemove = await dbContext.Books
                    .Where(b => b.AuthorId == authorId)
                    .ToListAsync();

                if (!booksToRemove.Any())
                {
                    logger.LogInformation("No Books found for Author with ID: {Id}", authorId);
                    return false;
                }

                dbContext.Books.RemoveRange(booksToRemove);
                await dbContext.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Unexpected error while deleting books for Author with ID: {Id}", authorId);
                throw new RepositoryException("Unexpected error occurred while deleting books.", ex);
            }
        }

        public async Task<IEnumerable<Author>> GetAll()
        {
            try
            {
                return await dbContext.Authors.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error occurred while fetching Authors.");
                throw new RepositoryException("Unexpected error occurred while fetching Authors.", ex);
            }
        }

        public async Task<List<Book>> GetBooksByAuthorId(int authorId)
        {
            try
            {
                return await dbContext.Books
                    .Where(b => b.AuthorId == authorId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while fetching books for Author ID: {Id}", authorId);
                throw new RepositoryException("Unexpected error occurred while fetching books.", ex);
            }
        }

        public async Task<Author?> GetById(int id)
        {
            try
            {
                return await dbContext.Authors.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.AuthorId == id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error occurred while fetching Author with ID: {Id}", id);
                throw new RepositoryException("Unexpected error occurred while fetching Author.", ex);
            }
        }

        public async Task<bool> Insert(Author author)
        {
            try
            {
                await dbContext.Authors.AddAsync(author);
                return await dbContext.SaveChangesAsync() > 0;
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Database update error while inserting Author.");
                throw new RepositoryException("Database error occurred while inserting Author.", ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error occurred while inserting Author.");
                throw new RepositoryException("Unexpected error occurred while inserting Author.", ex);
            }
        }

        public async Task<bool> Update(Author author)
        {
            try
            {
                dbContext.Authors.Update(author);
                return await dbContext.SaveChangesAsync() > 0;
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Database update error while updating Author with ID: {Id}", author.AuthorId);
                throw new RepositoryException("Database error occurred while updating Author.", ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error occurred while updating Author with ID: {Id}", author.AuthorId);
                throw new RepositoryException("Unexpected error occurred while updating Author.", ex);
            }
        }
    }
}
