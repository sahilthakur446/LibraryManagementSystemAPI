using LibraryManagementSystem.DTOs.Book;
using LibraryManagementSystem.Exceptions;
using LibraryManagementSystem.Mappers;
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
                var books = await dbContext.Books.AsNoTracking().Include(b => b.Author).Include(b => b.Category).ToListAsync();
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
                var book = await dbContext.Books.Include(b => b.Author).Include(b => b.Category).FirstOrDefaultAsync(b => b.BookId == id);
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
                await transaction.CommitAsync();
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

                    // Re-fetch the book with Author and Category populated
                    var completeBook = await dbContext.Books
                        .Include(b => b.Author)
                        .Include(b => b.Category)
                        .Include(b => b.BookCopies)
                        .FirstOrDefaultAsync(b => b.BookId == book.BookId);

                    addedBooks.Add(completeBook);
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
                var booksWithCopies = await dbContext.Books
                    .AsNoTracking()
                    .Include(b => b.BookCopies)
                    .Include(b => b.Author)
                    .Include(b => b.Category)
                    .ToListAsync();

                if (booksWithCopies is null || booksWithCopies.Count == 0)
                {
                    _logger.LogInformation("No books with copies were found in the database.");
                }

                return booksWithCopies;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all books with copies.");
                throw new RepositoryException("An error occurred while retrieving books from the database.", ex);
            }
        }

        public async Task<Book> GetBookWithCopies(int id)
        {
            try
            {
                var book = await dbContext.Books
                    .Include(b => b.BookCopies)
                    .Include(b => b.Author)
                    .Include(b => b.Category)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(b => b.BookId == id);

                if (book is null)
                {
                    _logger.LogWarning("Book with ID {Id} was not found.", id);
                    return null;
                }

                return book;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the book with ID {Id}.", id);
                throw new RepositoryException("An error occurred while fetching the book.", ex);
            }
        }
        public async Task<BookCopy?> GetBookCopyById(int id, bool getRelatedBook = true)
        {
            try
            {
                BookCopy? bookCopy;

                if (getRelatedBook)
                {
                    bookCopy = await dbContext.BookCopies
                        .Where(bc => bc.CopyId == id)
                        .Include(b => b.Book)
                        .FirstOrDefaultAsync();
                }
                else
                {
                    bookCopy = await dbContext.BookCopies.FindAsync(id);
                }

                if (bookCopy is null)
                {
                    _logger.LogWarning("Book copy not found with ID: {Id}", id);
                    return null;
                }

                return bookCopy;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching book copy by ID: {Id}", id);
                throw new RepositoryException("Error fetching book copy.", ex);
            }
        }

        public async Task<bool> AddBookCopy(BookCopy bookCopy, Book book)
        {
            try
            {
                await dbContext.BookCopies.AddAsync(bookCopy);
                dbContext.Attach(book);
                dbContext.Entry(book).Property(b => b.TotalCopies).IsModified = true;
                dbContext.Entry(book).Property(b => b.AvailableCopies).IsModified = true;
                var result = await dbContext.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding a new BookCopy.");
                throw new RepositoryException("An error occurred while saving the Book Copy.", ex);
            }
        }

        public async Task<bool> UpdateBookCopy(BookCopy bookCopy, Book book)
        {
            try
            {
                dbContext.Attach(book);
                dbContext.Entry(book).Property(b => b.TotalCopies).IsModified = true;
                dbContext.Entry(book).Property(b => b.AvailableCopies).IsModified = true;

                dbContext.BookCopies.Update(bookCopy);

                var result = await dbContext.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating BookCopy with ID: {Id}.", bookCopy.CopyId);
                throw new RepositoryException("An error occurred while updating the Book Copy.", ex);
            }
        }

        public async Task<bool> DeleteBookCopy(BookCopy bookCopy,Book book)
        {
            try
            {
                dbContext.BookCopies.Remove(bookCopy);
                dbContext.Attach(book);
                dbContext.Entry(book).Property(b => b.TotalCopies).IsModified = true;
                dbContext.Entry(book).Property(b => b.AvailableCopies).IsModified = true;
                var result = await dbContext.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting BookCopy with ID: {Id}.", bookCopy.CopyId);
                throw new RepositoryException("An error occurred while deleting the Book Copy.", ex);
            }
        }
    }
}
