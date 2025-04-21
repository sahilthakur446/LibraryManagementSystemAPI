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

        public async Task<BorrowedBook> BorrowBookAsync(int bookId, int userId)
        {
            var borrowedBook = new BorrowedBook
            {
                BookId = bookId,
                UserId = userId,
                DueDate = DateTime.UtcNow.AddDays(14).Date,
                StatusId = (int)BorrowedBookStatusEnum.Borrowed
            };

            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                var book = await dbContext.Books.FirstOrDefaultAsync(b => b.BookId == bookId);
                if (book == null)
                {
                    logger.LogWarning("Book with ID {BookId} not found in BorrowBook", bookId);
                    throw new RepositoryException("Book not found");
                }

                if (book.AvailableCopies <= 0)
                {
                    throw new RepositoryException("No available copies left to borrow");
                }

                book.AvailableCopies -= 1;

                dbContext.Books.Update(book);
                dbContext.BorrowedBooks.Add(borrowedBook);

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                // Now reload the BorrowedBook with navigation properties
                var borrowedBookWithDetails = await dbContext.BorrowedBooks
                    .Include(bb => bb.Book)
                    .Include(bb => bb.User)
                    .Include(bb => bb.Status)
                    .FirstOrDefaultAsync(bb => bb.BorrowId == borrowedBook.BorrowId);

                return borrowedBookWithDetails!;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                logger.LogError("Concurrency conflict while borrowing BookId {BookId}", bookId);
                await transaction.RollbackAsync();
                throw new RepositoryException("Book was modified by another user. Please try again.", ex);
            }
            catch (DbUpdateException ex)
            {
                logger.LogError("Database error while borrowing BookId {BookId}", bookId);
                await transaction.RollbackAsync();
                throw new RepositoryException("Database update failed", ex);
            }
            catch (Exception ex)
            {
                logger.LogError("Unexpected error while borrowing BookId {BookId}", bookId);
                await transaction.RollbackAsync();
                throw new RepositoryException("Unexpected error occurred", ex);
            }
        }

        public async Task<BorrowedBook> ReturnBookAsync(int bookId, int userId)
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                // Fetch the book
                var book = await dbContext.Books.FirstOrDefaultAsync(b => b.BookId == bookId);
                if (book == null)
                {
                    logger.LogWarning("Book with ID {BookId} not found in ReturnBook", bookId);
                    throw new RepositoryException("Book not found");
                }

                // Get active borrowed record for this book and user
                var borrowedBook = await dbContext.BorrowedBooks
                    .FirstOrDefaultAsync(bb => bb.BookId == bookId && bb.UserId == userId && bb.StatusId == (int)BorrowedBookStatusEnum.Borrowed);

                if (borrowedBook == null)
                {
                    logger.LogWarning("Borrowed record not found for BookId {BookId} and UserId {UserId}", bookId, userId);
                    throw new RepositoryException("Borrowed record not found");
                }

                // Update book stock and status
                book.AvailableCopies += 1;

                borrowedBook.StatusId = (int)BorrowedBookStatusEnum.Returned;
                borrowedBook.ReturnDate = DateTime.UtcNow.Date;

                borrowedBook.FineAmount = borrowedBook.DueDate < borrowedBook.ReturnDate
                    ? (borrowedBook.ReturnDate.Value - borrowedBook.DueDate).Days * 5
                    : 0;

                dbContext.Books.Update(book);
                dbContext.BorrowedBooks.Update(borrowedBook);


                await dbContext.SaveChangesAsync(); // Important before committing transaction
                await transaction.CommitAsync();

                // Return borrowed book with related data
                var borrowedBookWithDetails = await dbContext.BorrowedBooks
                    .Include(bb => bb.Book)
                    .Include(bb => bb.User)
                    .FirstOrDefaultAsync(bb => bb.BorrowId == borrowedBook.BorrowId);

                return borrowedBookWithDetails!;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "Concurrency conflict while returning BookId {BookId}", bookId);
                throw new RepositoryException("The book was modified by another user. Please try again.", ex);
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "Database update error while returning BookId {BookId}", bookId);
                throw new RepositoryException("Database update failed during book return.", ex);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "Unexpected error occurred while returning BookId {BookId}", bookId);
                throw new RepositoryException("An unexpected error occurred while returning the book.", ex);
            }
        }
        public async Task<BorrowedBook> ReturnBookAsync(int borrowingId)
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                var borrowedBook = await dbContext.BorrowedBooks
                    .Where(bb => bb.BorrowId == borrowingId)
                    .Include(bb => bb.Book)
                    .Include(bb => bb.User)
                    .Include(bb => bb.Status)
                    .FirstOrDefaultAsync();

                if (borrowedBook == null)
                {
                    logger.LogWarning("Borrowed record not found for BorrowingId {BorrowingId}", borrowingId);
                    throw new RepositoryException("Borrowed record not found");
                }

                if (borrowedBook.Status.StatusId == (int)BorrowedBookStatusEnum.Returned)
                {
                    logger.LogWarning("Book with BorrowingId {BorrowingId} has already been returned", borrowingId);
                    throw new RepositoryException("This book has already been returned.");
                }


                // Store IDs for logging in case needed
                var bookId = borrowedBook.Book?.BookId ?? 0;
                var userId = borrowedBook.User?.UserId ?? 0;

                // Update book stock and status
                borrowedBook.Book.AvailableCopies += 1;
                borrowedBook.StatusId = (int)BorrowedBookStatusEnum.Returned;
                borrowedBook.ReturnDate = DateTime.UtcNow.Date;

                borrowedBook.FineAmount = borrowedBook.DueDate < borrowedBook.ReturnDate
                    ? (borrowedBook.ReturnDate.Value - borrowedBook.DueDate).Days * 5
                    : 0;

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
            finally 
            {
                await transaction.RollbackAsync();
            }

        }


        public async Task<IList<BorrowedBook>> GetBorrowedBooksByUserIdAsync(int userId)
        {
            try
            {
                return await dbContext.BorrowedBooks
                    .Where(bb => bb.UserId == userId && bb.StatusId == (int)BorrowedBookStatusEnum.Borrowed || bb.StatusId == (int)BorrowedBookStatusEnum.Overdue)
                    .Include(bb => bb.User)
                    .Include(bb => bb.Book)
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
                    .Include(bb => bb.Book)
                    .ToListAsync();
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
                    .Include(bb => bb.Book)
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
