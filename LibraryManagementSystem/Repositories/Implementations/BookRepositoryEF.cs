using LibraryManagementSystem.Exceptions;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Repositories.EF
{
    public class BookRepositoryEF : IBookRepository
    {
        private readonly LibraryDbContext dbContext;
        private readonly ILogger<BookRepositoryEF> _logger;

        public BookRepositoryEF(LibraryDbContext dbContext, ILogger<BookRepositoryEF> logger)
        {
            this.dbContext = dbContext;
            _logger = logger;
        }

        public async Task<IEnumerable<Book?>> GetAll()
        {
            try
            {
                var books = await dbContext.Books.AsNoTracking().Include(b => b.BookCopies).Include(b => b.Category).ToListAsync();
                if (books == null || !books.Any())
                {
                    _logger.LogInformation("No books found in the database.");
                    return Enumerable.Empty<Book>();
                }
                return books;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Repository: Error fetching all books in GetAll.");
                throw new RepositoryException("Repository: Error fetching books. Ex", ex);
            }
        }

        public async Task<Book?> GetById(int id)
        {
            try
            {
                var book = await dbContext.Books.FindAsync(id);
                if (book is null)
                {
                    _logger.LogWarning("Book not found with ID: {Id}", id);
                    return null;
                }
                return book;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching book by ID: {Id}", id);
                throw new RepositoryException("Error fetching book.", ex);
            }
        }

        public async Task<bool> Insert(Book book)
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                await dbContext.Books.AddAsync(book);

                // Save the book to the database and get the BookId
                await dbContext.SaveChangesAsync();

                var bookTotalCopies = book.TotalCopies;

                // Generate the list of BookCopy objects based on the total copies
                var bookCopyList = Enumerable.Range(0, bookTotalCopies)
                    .Select(count => new BookCopy { BookId = book.BookId, IsAvailable = true })
                    .ToList();

                await dbContext.BookCopies.AddRangeAsync(bookCopyList);

                int result = await dbContext.SaveChangesAsync();

                if (result <= 0)
                {
                    _logger.LogWarning("Insert operation failed for book: {@Book}", book);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error inserting book: {@Book}", book);
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
                    _logger.LogWarning("Update operation failed for book: {@Book}", book);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating book: {@Book}", book);
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
                    _logger.LogWarning("Attempted to delete non-existing book with ID: {Id}", id);
                    return false;
                }

                dbContext.Books.Remove(book);
                var result = await dbContext.SaveChangesAsync();
                if (result <= 0)
                {
                    _logger.LogWarning("Delete operation failed for book with ID: {Id}", id);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting book with ID: {Id}", id);
                throw new RepositoryException("Error deleting book.", ex);
            }
        }

        public async Task<List<Book>> AddBooks(List<Book> books)
        {
            var addedBooks = new List<Book>();

            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                foreach (var book in books)
                {
                    await dbContext.Books.AddAsync(book);
                    await dbContext.SaveChangesAsync();

                    var bookCopyList = Enumerable.Range(0, book.TotalCopies)
                        .Select(count => new BookCopy { BookId = book.BookId, IsAvailable = true })
                        .ToList();

                    await dbContext.BookCopies.AddRangeAsync(bookCopyList);
                    await dbContext.SaveChangesAsync();

                    addedBooks.Add(book);
                }

                await transaction.CommitAsync();
                return addedBooks;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Transaction failed while adding books.");
                throw new RepositoryException("Transaction failed while adding books.", ex);
            }
        }

        public async Task<List<Book>> GetAllBooksWithCopies()
        {
            try
            {
                var bookWithCopies = await dbContext.Books.AsNoTracking().Include(b => b.BookCopies).ToListAsync();
                if (bookWithCopies == null || !bookWithCopies.Any())
                {
                    _logger.LogInformation("No books found with copies.");
                    return bookWithCopies;
                }
                return bookWithCopies;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Error Occurred in GetAllBooksWithCopies method of BookRepository");
                throw new RepositoryException("Database error occured", ex);
            }
        }

        public Task<List<Book>> GetBookAllCopies()
        {
            throw new NotImplementedException();
        }

        public Task<bool> AddBookCopy(Book book)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateBookCopy(Book book)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteBookCopy(Book book)
        {
            throw new NotImplementedException();
        }
    }
}
