using LibraryManagementSystem.Exceptions;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Repositories.EF
{
    public class BookRepositoryEF : IBookRepository
    {
        private readonly LibraryDbContext dbContext;
        private readonly ILogger<BookRepositoryEF> logger;

        public BookRepositoryEF(LibraryDbContext dbContext, ILogger<BookRepositoryEF> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        public async Task<IEnumerable<Book?>> GetAll()
        {
            try
            {
                var books = await dbContext.Books.AsNoTracking().Include(b => b.Author).Include(b => b.Category).ToListAsync();
                if (books == null || !books.Any())
                {
                    logger.LogInformation("No books found in the database.");
                    return Enumerable.Empty<Book>();
                }
                return books;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching all books in GetAll.");
                throw new RepositoryException("Error fetching books.", ex);
            }
        }

        public async Task<Book?> GetById(int id)
        {
            try
            {
                var book = await dbContext.Books.FindAsync(id);
                if (book is null)
                {
                    logger.LogWarning("Book not found with ID: {Id}", id);
                    return null;
                }
                return book;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching book by ID: {Id}", id);
                throw new RepositoryException("Error fetching book.", ex);
            }
        }

        public async Task<bool> Insert(Book book)
        {
            try
            {
                await dbContext.Books.AddAsync(book);
                var result = await dbContext.SaveChangesAsync();
                if (result <= 0)
                {
                    logger.LogWarning("Insert operation failed for book: {@Book}", book);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error inserting book: {@Book}", book);
                throw new RepositoryException("Error inserting book.", ex);
            }
        }

        public async Task<bool> Update(Book book)
        {
            try
            {
                dbContext.Books.Update(book);
                var result = await dbContext.SaveChangesAsync();
                if (result <= 0)
                {
                    logger.LogWarning("Update operation failed for book: {@Book}", book);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating book: {@Book}", book);
                throw new RepositoryException("Error updating book.", ex);
            }
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                var book = await GetById(id);
                if (book is null)
                {
                    logger.LogWarning("Attempted to delete non-existing book with ID: {Id}", id);
                    return false;
                }

                dbContext.Books.Remove(book);
                var result = await dbContext.SaveChangesAsync();
                if (result <= 0)
                {
                    logger.LogWarning("Delete operation failed for book with ID: {Id}", id);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting book with ID: {Id}", id);
                throw new RepositoryException("Error deleting book.", ex);
            }
        }
    }
}
