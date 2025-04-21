using LibraryManagementSystem.DTOs.Borrowing;
using LibraryManagementSystem.Enums;
using LibraryManagementSystem.Exceptions;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Repositories.EF
{
    public class BorrowingRepository : IBorrowingRepository
    {
        private readonly LibraryDbContext dbContext;
        private readonly ILogger<BorrowingRepository> logger;

        public BorrowingRepository(LibraryDbContext dbContext, ILogger<BorrowingRepository> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        public async Task<int> GetAvailableBookCopiesAsync(int bookId)
        {
            try
            {
                var book = await dbContext.Books.FirstOrDefaultAsync(b => b.BookId == bookId);
                if (book == null)
                {
                    logger.LogWarning("Book with ID {BookId} not found in GetAvailableBookCopiesAsync", bookId);
                    throw new RepositoryException("Book not found");
                }
                return book.AvailableCopies;
            }
            catch (Exception ex)
            {
                logger.LogError("Error in GetAvailableBookCopiesAsync for BookId {BookId}", bookId);
                throw;
            }
        }

        public async Task<BorrowedBook> BorrowBookAsync(int bookCopyId, int userId)
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                // Fetch book copy and book
                var bookCopy = await dbContext.BookCopies
                    .Include(bc => bc.Book)
                    .FirstOrDefaultAsync(bc => bc.CopyId == bookCopyId);

                if (bookCopy == null)
                {
                    logger.LogWarning("BookCopy with ID {BookCopyId} not found", bookCopyId);
                    throw new RepositoryException("Book copy not found.");
                }

                var book = bookCopy.Book;
                if (book == null)
                {
                    logger.LogWarning("Book not linked to BookCopy ID {BookCopyId}", bookCopyId);
                    throw new RepositoryException("Book information is missing.");
                }

                if (!bookCopy.IsAvailable)
                {
                    logger.LogWarning("BookCopy ID {BookCopyId} is already borrowed", bookCopyId);
                    throw new RepositoryException("This book copy is already borrowed.");
                }

                if (book.AvailableCopies <= 0)
                {
                    logger.LogWarning("No available copies for Book ID {BookId}", book.BookId);
                    throw new RepositoryException("No available copies left to borrow.");
                }

                bool alreadyBorrowed = await dbContext.BorrowedBooks.AnyAsync(bb =>
                    bb.BookCopyId == bookCopyId &&
                    bb.UserId == userId &&
                    bb.StatusId == (int)BorrowedBookStatusEnum.Borrowed);

                if (alreadyBorrowed)
                {
                    throw new RepositoryException("This book is already borrowed by the user.");
                }

                // Create the borrowed book record
                var borrowedBook = new BorrowedBook
                {
                    BookCopyId = bookCopyId,
                    UserId = userId,
                    DueDate = DateTime.UtcNow.AddDays(14).Date,
                    StatusId = (int)BorrowedBookStatusEnum.Borrowed
                };

                // Update stock and availability
                book.AvailableCopies -= 1;
                bookCopy.IsAvailable = false;

                dbContext.BorrowedBooks.Add(borrowedBook);
                dbContext.Books.Update(book);
                dbContext.BookCopies.Update(bookCopy);

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                logger.LogInformation("BookCopy ID {BookCopyId} borrowed by User ID {UserId}", bookCopyId, userId);

                // Reload the BorrowedBook with related entities
                var borrowedBookWithDetails = await dbContext.BorrowedBooks
                    .Include(bb => bb.BookCopy)
                        .ThenInclude(bc => bc.Book)
                    .Include(bb => bb.User)
                    .Include(bb => bb.BorrowedBookStatus)
                    .FirstOrDefaultAsync(bb => bb.BorrowId == borrowedBook.BorrowId);

                return borrowedBookWithDetails!;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "Concurrency conflict while borrowing BookCopyId {BookCopyId}", bookCopyId);
                throw new RepositoryException("Book was modified by another user. Please try again.", ex);
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "Database error while borrowing BookCopyId {BookCopyId}", bookCopyId);
                throw new RepositoryException("Database update failed while borrowing the book.", ex);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "Unexpected error while borrowing BookCopyId {BookCopyId}", bookCopyId);
                throw new RepositoryException("An unexpected error occurred while borrowing the book.", ex);
            }
        }

        public async Task<BorrowedBook> ReturnBookAsync(int copyBookId, int userId)
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                // Fetch the book copy along with its Book navigation property
                var copyBook = await dbContext.BookCopies
                    .Include(bc => bc.Book)
                    .FirstOrDefaultAsync(bc => bc.CopyId == copyBookId);

                if (copyBook == null)
                {
                    throw new RepositoryException("Book copy not found.");
                }

                var book = copyBook.Book;
                int bookId = book.BookId;

                // Find the borrowed record for this user and book copy that is not yet returned
                var borrowedBook = await dbContext.BorrowedBooks
                    .FirstOrDefaultAsync(bb => bb.BookCopyId == copyBookId &&
                                               bb.UserId == userId &&
                                               bb.StatusId != (int)BorrowedBookStatusEnum.Returned);

                if (borrowedBook == null)
                {
                    throw new RepositoryException("No active borrowed record found for this book and user.");
                }

                // Update book stock
                book.AvailableCopies += 1;
                copyBook.IsAvailable = true;
                // Mark as returned
                borrowedBook.StatusId = (int)BorrowedBookStatusEnum.Returned;
                borrowedBook.ReturnDate = DateTime.UtcNow.Date;

                // Calculate fine if overdue
                if (borrowedBook.ReturnDate > borrowedBook.DueDate)
                {
                    borrowedBook.FineAmount = (borrowedBook.ReturnDate.Value - borrowedBook.DueDate).Days * 5;
                }
                else
                {
                    borrowedBook.FineAmount = 0;
                }

                // Save changes
                dbContext.Books.Update(book);
                dbContext.BorrowedBooks.Update(borrowedBook);

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                // Return the updated borrowed book with related details
                var borrowedBookWithDetails = await dbContext.BorrowedBooks
                    .Include(bb => bb.BookCopy)
                        .ThenInclude(bc => bc.Book)
                    .Include(bb => bb.User)
                    .FirstOrDefaultAsync(bb => bb.BorrowId == borrowedBook.BorrowId);

                return borrowedBookWithDetails!;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "Concurrency conflict while returning BookCopyId {CopyBookId} for UserId {UserId}", copyBookId, userId);
                throw new RepositoryException("The book was modified by another user. Please try again.", ex);
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "Database update error while returning BookCopyId {CopyBookId} for UserId {UserId}", copyBookId, userId);
                throw new RepositoryException("Database update failed during book return.", ex);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "Unexpected error occurred while returning BookCopyId {CopyBookId} for UserId {UserId}", copyBookId, userId);
                throw new RepositoryException("An unexpected error occurred while returning the book.", ex);
            }
        }

        public async Task<BorrowedBook> ReturnBookAsync(int borrowingId)
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                // Fetch the borrowed book with full related data
                var borrowedBook = await dbContext.BorrowedBooks
                    .Include(bb => bb.BookCopy)
                        .ThenInclude(bc => bc.Book)
                    .Include(bb => bb.User)
                    .FirstOrDefaultAsync(bb => bb.BorrowId == borrowingId);

                if (borrowedBook == null)
                {
                    logger.LogWarning("Borrowed record not found for BorrowingId {BorrowingId}", borrowingId);
                    throw new RepositoryException("Borrowed record not found.");
                }

                if (borrowedBook.StatusId == (int)BorrowedBookStatusEnum.Returned)
                {
                    logger.LogWarning("Book with BorrowingId {BorrowingId} has already been returned", borrowingId);
                    throw new RepositoryException("This book has already been returned.");
                }

                var book = borrowedBook.BookCopy?.Book;

                if (book == null)
                {
                    logger.LogError("Book data is missing for BorrowingId {BorrowingId}", borrowingId);
                    throw new RepositoryException("Book data not found for borrowed record.");
                }

                // Extract IDs for logging
                var bookId = book.BookId;
                var userId = borrowedBook.User?.UserId ?? 0;

                // Update book stock
                book.AvailableCopies += 1;

                // Update borrow record
                borrowedBook.StatusId = (int)BorrowedBookStatusEnum.Returned;
                borrowedBook.ReturnDate = DateTime.UtcNow.Date;

                if (borrowedBook.ReturnDate > borrowedBook.DueDate)
                {
                    borrowedBook.FineAmount = (borrowedBook.ReturnDate.Value - borrowedBook.DueDate).Days * 5;
                }
                else
                {
                    borrowedBook.FineAmount = 0;
                }

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                logger.LogInformation("Book with ID {BookId} returned by User ID {UserId}", bookId, userId);
                return borrowedBook;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "Concurrency conflict while returning book for BorrowingId {BorrowingId}", borrowingId);
                throw new RepositoryException("The book was modified by another user. Please try again.", ex);
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "Database update error while returning book for BorrowingId {BorrowingId}", borrowingId);
                throw new RepositoryException("Database update failed during book return.", ex);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "Unexpected error occurred while returning book for BorrowingId {BorrowingId}", borrowingId);
                throw new RepositoryException("An unexpected error occurred while returning the book.", ex);
            }
        }


        public async Task<IList<BorrowedBook>> GetBorrowedBooksByUserIdAsync(int userId)
        {
            try
            {
                return await dbContext.BorrowedBooks
                    .Where(bb => bb.UserId == userId && bb.StatusId == (int)BorrowedBookStatusEnum.Borrowed || bb.StatusId == (int)BorrowedBookStatusEnum.Overdue)
                    .Include(bb => bb.User)
                    .Include(bb => bb.BookCopy)
                    .ThenInclude(bc => bc.Book)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError("Error fetching borrowed books for UserId {UserId}", userId);
                throw new RepositoryException("Error fetching borrowed books", ex);
            }
        }

        public async Task<IList<BorrowedBook>> GetAllBorrowedBooksAsync()
        {
            try
            {
                return await dbContext.BorrowedBooks
                    .Where(bb => bb.StatusId == (int)BorrowedBookStatusEnum.Borrowed)
                    .Include(bb => bb.User)
                    .Include(bb => bb.BookCopy)
                    .ThenInclude(bc => bc.Book).ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError("Error fetching all borrowed books");
                throw new RepositoryException("Error fetching borrowed books", ex);
            }
        }

        public async Task<IList<BorrowedBook>> GetAllOverDueBooksAsync()
        {
            try
            {
                return await dbContext.BorrowedBooks
                    .Where(bb => bb.StatusId == (int)BorrowedBookStatusEnum.Overdue)
                    .Include(bb => bb.User)
                   .Include(bb => bb.BookCopy)
                    .ThenInclude(bc => bc.Book)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError("Error fetching overdue books");
                throw new RepositoryException("Error fetching overdue books", ex);
            }
        }

        public async Task<int> GetRemainingFineByUserIdAsync(int userId)
        {
            try
            {
                var fine = await dbContext.BorrowedBooks
                            .Where(bb => bb.UserId == userId && bb.StatusId == (int)BorrowedBookStatusEnum.Overdue)
                            .SumAsync(bb => bb.FineAmount);
                return fine ?? 0;
            }
            catch (Exception)
            {
                logger.LogError("Error Calculating the Fine");
                throw;
            }
        }

        public Task<bool> HasUserAlreadyBorrowedSameBookAsync(int bookId, int userId)
        {
            throw new NotImplementedException();
        }
    }
}
