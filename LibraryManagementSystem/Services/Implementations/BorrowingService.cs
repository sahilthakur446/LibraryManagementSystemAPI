using LibraryManagementSystem.DTOs.Borrowing;
using LibraryManagementSystem.Exceptions;
using LibraryManagementSystem.Mappers;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories.EF;
using LibraryManagementSystem.Repositories.Interfaces;
using LibraryManagementSystem.Services.Interfaces;
using System.Collections.Generic;

namespace LibraryManagementSystem.Services.Implementations
{
    public class BorrowingService : IBorrowingService
    {
        private readonly IBorrowingRepository borrowingRepository;
        private readonly IEmailService emailService;
        private readonly ILogger<BorrowingService> logger;

        public BorrowingService(IBorrowingRepository borrowingRepository, IEmailService emailService, ILogger<BorrowingService> logger)
        {
            this.borrowingRepository = borrowingRepository;
            this.emailService = emailService;
            this.logger = logger;
        }

        public async Task<BorrowingResponseDTO> BorrowBookAsync(int bookId, int userId, int bookCopyId = 0)
        {
            try
            {
                var borrowedBooks = await borrowingRepository.GetBorrowedBooksByUserIdAsync(userId);
                if (borrowedBooks.Count >= 3)
                {
                    throw new BusinessExceptions("Borrowing limit reached. You cannot borrow more than 3 books.", StatusCodes.Status200OK);
                }
                var isSameBookAlreadyBorrowedByUser = await IsSameBookAlreadyBorrowedByUser(bookId, userId);
                if (isSameBookAlreadyBorrowedByUser)
                {
                    throw new BusinessExceptions("This book is already borrowed by the user.", StatusCodes.Status400BadRequest);
                }
                var availableCopies = await borrowingRepository.GetAvailableBookCopiesAsync(bookId);
                if (availableCopies == 0)
                {
                    throw new BusinessExceptions("No available copies of this book right now.", StatusCodes.Status400BadRequest);
                }

                var outstandingFine = await borrowingRepository.GetRemainingFineByUserIdAsync(userId);
                if (outstandingFine > 100)
                {
                    throw new BusinessExceptions("Outstanding fine exceeds limit. Please repay your dues before borrowing more books.", StatusCodes.Status400BadRequest);
                }
                var borrowedBook = await borrowingRepository.BorrowBookAsync(bookId, userId, bookCopyId);
                string userName = $"{borrowedBook.User.FirstName} {borrowedBook.User.LastName}";
                DateTime returnDate = borrowedBook.BorrowDate.AddDays(14);
                await emailService.SendBookIssuedEmailAsync(
                    borrowedBook.User.Email,
                    userName,
                    borrowedBook.BorrowDate,
                    returnDate
                );
                return BorrowingMapper.FromModel(borrowedBook);
            }
            catch (RepositoryException ex) when (ex.Message == "Book not found")
            {
                logger.LogWarning("Book with ID {BookId} not found (business level)", bookId);
                throw new BusinessExceptions("Book not found", StatusCodes.Status404NotFound);
            }
            catch (RepositoryException ex)
            {
                logger.LogError(ex, "Error while borrowing book for UserId: {UserId}, BookId: {BookId}", userId, bookId);
                throw new BusinessExceptions("An error occurred while processing your borrow request. Please try again later.", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<BorrowingResponseDTO> ReturnBookAsync(int copyBookId, int userId)
        {
            try
            {
                var returnedBook = await borrowingRepository.ReturnBookAsync(copyBookId, userId);
                string userName = $"{returnedBook.User.FirstName} {returnedBook.User.LastName}";
                await emailService.SendBookReturnedEmailAsync(
                    returnedBook.User.Email,
                    userName,
                    returnedBook.BorrowDate, returnedBook.DueDate, DateTime.UtcNow.Date
                );
                return BorrowingMapper.FromModel(returnedBook);
            }
            catch (RepositoryException ex)
            {
                logger.LogError(ex, "Error while returning book for UserId: {UserId},CopyBookId;{CopyBookId}", userId, copyBookId);
                throw new BusinessExceptions("An error occurred while processing your return request. Please try again later.", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<BorrowingResponseDTO> ReturnBookAsync(int borrowingId)
        {
            try
            {
                var returnedBook = await borrowingRepository.ReturnBookAsync(borrowingId);

                if (returnedBook == null)
                {
                    logger.LogWarning("ReturnBookAsync: Returned book or user info is missing for BorrowingId {BorrowingId}", borrowingId);
                    throw new BusinessExceptions("Borrowing Record Not Found.", StatusCodes.Status404NotFound);
                }

                var userId = returnedBook.User.UserId;
                var bookCopyId = returnedBook.BookCopy?.CopyId ?? 0;
                string userName = $"{returnedBook.User.FirstName} {returnedBook.User.LastName}";

                await emailService.SendBookReturnedEmailAsync(
                    returnedBook.User.Email,
                    userName,
                    returnedBook.BorrowDate,
                    returnedBook.DueDate,
                    DateTime.UtcNow.Date
                );

                logger.LogInformation("ReturnBookAsync: Book Copy ID {BookId} successfully returned by User ID {UserId}", bookCopyId, userId);

                return BorrowingMapper.FromModel(returnedBook);
            }
            catch (RepositoryException ex) when (ex.Message == "This book has already been returned.")
            {
                logger.LogWarning(ex, "Book already returned for BorrowingId: {BorrowingId}", borrowingId);
                throw new BusinessExceptions(
                    ex.Message,
                    StatusCodes.Status400BadRequest
                );
            }

            catch (RepositoryException ex)
            {
                logger.LogError(ex, "Error while returning book for BorrowingId: {BorrowingId}", borrowingId);
                throw new BusinessExceptions(
                    "An error occurred while processing your return request. Please try again later.",
                    StatusCodes.Status500InternalServerError
                );
            }
        }


        public async Task<IList<BorrowingResponseDTO>> GetAllBorrowedBooksAsync()
        {
            try
            {
                var borrowedBooks = await borrowingRepository.GetAllBorrowedBooksAsync();
                return borrowedBooks.Select(BorrowingMapper.FromModel).ToList();
            }
            catch (RepositoryException ex)
            {
                logger.LogError(ex, "Error while retrieving all borrowed books.");
                throw new BusinessExceptions("An error occurred while fetching borrowed books.", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<IList<BorrowingResponseDTO>> GetAllOverDueBooksAsync()
        {
            try
            {
                var overdueBooks = await borrowingRepository.GetAllOverDueBooksAsync();
                return overdueBooks.Select(BorrowingMapper.FromModel).ToList();
            }
            catch (RepositoryException ex)
            {
                logger.LogError(ex, "Error while retrieving all overdue books.");
                throw new BusinessExceptions("An error occurred while fetching overdue books.", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<int> GetAvailableBookCopiesAsync(int bookId)
        {
            try
            {
                return await borrowingRepository.GetAvailableBookCopiesAsync(bookId);
            }
            catch (RepositoryException ex)
            {
                logger.LogError(ex, "Error while fetching available copies for BookId: {BookId}", bookId);
                throw new BusinessExceptions("Could not fetch available copies.", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<IList<BorrowingResponseDTO>> GetBorrowedBooksByUserIdAsync(int userId)
        {
            try
            {
                var borrowedBooks = await borrowingRepository.GetBorrowedBooksByUserIdAsync(userId);
                return borrowedBooks.Select(BorrowingMapper.FromModel).ToList();
            }
            catch (RepositoryException ex)
            {
                logger.LogError(ex, "Error while fetching borrowed books for UserId: {UserId}", userId);
                throw new BusinessExceptions("Could not fetch user's borrowed books.", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<int> GetRemainingFineByUserIdAsync(int userId)
        {
            try
            {
                return await borrowingRepository.GetRemainingFineByUserIdAsync(userId);
            }
            catch (RepositoryException ex)
            {
                logger.LogError(ex, "Error while calculating remaining fine for UserId: {UserId}", userId);
                throw new BusinessExceptions("Could not fetch remaining fine.", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<bool> IsSameBookAlreadyBorrowedByUser(int bookId, int userId)
        {
            try
            {
                var borrowedBooks = await borrowingRepository.GetBorrowedBooksByUserIdAsync(userId);

                if (borrowedBooks == null || !borrowedBooks.Any())
                {
                    return false;
                }

                return borrowedBooks.Any(b => b.BookCopy.BookId == bookId);
            }
            catch (RepositoryException ex)
            {
                logger.LogError(ex, "Error checking if the user has already borrowed the book. UserId: {UserId}, BookId: {BookId}", userId, bookId);
                throw new BusinessExceptions("An error occurred while checking the borrowed book status.", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<IList<BorrowingResponseDTO>> GetBorrowingsDueTomorrowAsync()
        {
            try
            {
                var borrowingsDueTomorrow = await borrowingRepository.GetAllBorrowedBooksDueTomorrowAsync();

                return borrowingsDueTomorrow
                    .Select(BorrowingMapper.FromModel)
                    .ToList();
            }
            catch (RepositoryException ex)
            {
                logger.LogError(ex, "Error while retrieving borrowings due tomorrow.");
                throw new BusinessExceptions("An error occurred while fetching borrowings due tomorrow.", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<IList<BorrowedBookEmailDTO>> GetBorrowedBooksDueTomorrowForEmailAsync()
        {
            try
            {
                var borrowingsDueTomorrow = await borrowingRepository.GetAllBorrowedBooksDueTomorrowForEmailAsync();

                return borrowingsDueTomorrow;
            }
            catch (RepositoryException ex)
            {
                logger.LogError(ex, "Error while retrieving borrowings due tomorrow for email.");
                throw new BusinessExceptions("An error occurred while fetching borrowings.", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<IList<BorrowedBookEmailDTO>> GetAllOverDueBooksForEmailAsync()
        {
            try
            {
                var overdueBorrowings = await borrowingRepository.GetAllOverDueBooksForEmailAsync();

                return overdueBorrowings;
            }
            catch (RepositoryException ex)
            {
                logger.LogError(ex, "Error while retrieving borrowings due tomorrow for email.");
                throw new BusinessExceptions("An error occurred while fetching borrowings.", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<bool> UpdateOverdueBooksStatusAsync()
        {
            try
            {
                bool updateSuccessful = await borrowingRepository.UpdateOverdueBooksStatusAsync();
                return updateSuccessful;
            }
            catch (RepositoryException ex)
            {
                logger.LogError(ex, "Error while updating overdue book statuses.");
                throw new BusinessExceptions("An error occurred while updating overdue book statuses.", StatusCodes.Status500InternalServerError, ex);
            }
        }
    }
}
