using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Moq;
using LibraryManagementSystem.Repositories.Interfaces;
using LibraryManagementSystem.Services.Interfaces;
using LibraryManagementSystem.Services.Implementations;
using Microsoft.Extensions.Logging;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Exceptions;
using Microsoft.AspNetCore.Http;

namespace LibraryManagementSystem.Tests.Services.BorrowingServiceTests
{
    public class BorrowingServiceTests_BorrowBook
    {
        [Fact]
        public async Task BorrowBook_ShouldThrow_WhenBorrowingLimitExceeded()
        {
            // Arrange
            var borrowingRepositoryMock = new Mock<IBorrowingRepository>();
            var emailServiceMock = new Mock<IEmailService>();
            var loggerMock = new Mock<ILogger<BorrowingService>>();
            var borrowingService = new BorrowingService(borrowingRepositoryMock.Object, emailServiceMock.Object, loggerMock.Object);

            int bookId = 1;
            int userId = 1;

            borrowingRepositoryMock.Setup(repo => repo.GetBorrowedBooksByUserIdAsync(userId))
                .ReturnsAsync(new List<BorrowedBook> { new BorrowedBook(), new BorrowedBook(), new BorrowedBook() });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessExceptions>(() => borrowingService.BorrowBookAsync(bookId, userId));
            Assert.Equal("Borrowing limit reached. You cannot borrow more than 3 books.", exception.Message);
        }

        [Fact]
        public async Task BorrowBook_ShouldThrow_WhenNoAvailableCopies()
        {
            // Arrange
            var borrowingRepositoryMock = new Mock<IBorrowingRepository>();
            var emailServiceMock = new Mock<IEmailService>();
            var loggerMock = new Mock<ILogger<BorrowingService>>();
            var borrowingService = new BorrowingService(borrowingRepositoryMock.Object, emailServiceMock.Object, loggerMock.Object);

            int bookId = 6;
            int userId = 1;
            int expectedCount = 0;

            borrowingRepositoryMock.Setup(repo => repo.GetBorrowedBooksByUserIdAsync(userId))
                .ReturnsAsync(new List<BorrowedBook>());
            borrowingRepositoryMock.Setup(repo => repo.GetAvailableBookCopiesAsync(bookId))
                .ReturnsAsync(expectedCount);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessExceptions>(() => borrowingService.BorrowBookAsync(bookId, userId));
            Assert.Equal("No available copies of this book right now.", exception.Message);
            Assert.Equal(StatusCodes.Status400BadRequest, exception.StatusCode);
        }

        [Fact]
        public async Task BorrowBook_ShouldThrow_WhenOutstandingFineExceedsLimit()
        {
            // Arrange
            var borrowingRepositoryMock = new Mock<IBorrowingRepository>();
            var emailServiceMock = new Mock<IEmailService>();
            var loggerMock = new Mock<ILogger<BorrowingService>>();
            var borrowingService = new BorrowingService(borrowingRepositoryMock.Object, emailServiceMock.Object, loggerMock.Object);

            int bookId = 6;
            int userId = 1;

            borrowingRepositoryMock.Setup(repo => repo.GetBorrowedBooksByUserIdAsync(userId))
                .ReturnsAsync(new List<BorrowedBook>());
            borrowingRepositoryMock.Setup(repo => repo.GetAvailableBookCopiesAsync(bookId))
                .ReturnsAsync(1);
            borrowingRepositoryMock.Setup(repo => repo.GetRemainingFineByUserIdAsync(userId))
                .ReturnsAsync(101);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessExceptions>(() => borrowingService.BorrowBookAsync(bookId, userId));
            Assert.Equal("Outstanding fine exceeds limit. Please repay your dues before borrowing more books.", exception.Message);
            Assert.Equal(StatusCodes.Status400BadRequest, exception.StatusCode);
        }

        [Fact]
        public async Task BorrowBook_ShouldReturn_WhenAllValidationsPass()
        {
            // Arrange
            var borrowingRepositoryMock = new Mock<IBorrowingRepository>();
            var emailServiceMock = new Mock<IEmailService>();
            var loggerMock = new Mock<ILogger<BorrowingService>>();
            var borrowingService = new BorrowingService(
                borrowingRepositoryMock.Object,
                emailServiceMock.Object,
                loggerMock.Object
            );

            int bookId = 6;
            int userId = 1;

            borrowingRepositoryMock.Setup(repo => repo.GetBorrowedBooksByUserIdAsync(userId))
                .ReturnsAsync(new List<BorrowedBook>());

            borrowingRepositoryMock.Setup(repo => repo.GetAvailableBookCopiesAsync(bookId))
                .ReturnsAsync(5);

            borrowingRepositoryMock.Setup(repo => repo.GetRemainingFineByUserIdAsync(userId))
                .ReturnsAsync(0);

            borrowingRepositoryMock.Setup(repo => repo.BorrowBookAsync(bookId, userId))
                .ReturnsAsync(new BorrowedBook
                {
                    BookId = bookId,
                    Book = new Book
                    {
                        Title = "Test Book",
                        AuthorId = 1,
                        Isbn = "1234567890",
                        CategoryId = 1,
                        PublishedYear = 2023,
                        TotalCopies = 5,
                        AvailableCopies = 5
                    },
                    UserId = userId,
                    BorrowDate = DateTime.Now,
                    ReturnDate = DateTime.Now.AddDays(14),
                    User = new User
                    {
                        FirstName = "John",
                        LastName = "Doe",
                        Email = "john.doe@example.com"
                    },
                    StatusId = 1,
                    Status = new BorrowedBookStatus
                    {
                        StatusName = "Borrowed"
                    }
                });

            emailServiceMock.Setup(email =>
                email.SendBookIssuedEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>())
            ).Returns(Task.CompletedTask);

            // Act
            var result = await borrowingService.BorrowBookAsync(bookId, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(bookId, result.BookId);
            Assert.Equal(userId, result.UserId);
            Assert.Equal("John Doe", result.UserName); // Assuming the mapper does this
            Assert.Equal("Test Book", result.BookName); // Assuming the mapper does this
            Assert.Equal("Borrowed", result.BorrowStatus); // Assuming the mapper does this
        }

        [Fact]
        public async Task BorrowBook_ShouldThrow_WhenRepositoryThrowsBookNotFound()
        {
            // Arrange
            var borrowingRepositoryMock = new Mock<IBorrowingRepository>();
            var emailServiceMock = new Mock<IEmailService>();
            var loggerMock = new Mock<ILogger<BorrowingService>>();
            var borrowingService = new BorrowingService(borrowingRepositoryMock.Object, emailServiceMock.Object, loggerMock.Object);

            int bookId = 1;
            int userId = 1;

            borrowingRepositoryMock.Setup(repo => repo.GetBorrowedBooksByUserIdAsync(userId))
                .ThrowsAsync(new RepositoryException("Book not found"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessExceptions>(() => borrowingService.BorrowBookAsync(bookId, userId));
            Assert.Equal("Book not found", exception.Message);
            Assert.Equal(StatusCodes.Status404NotFound, exception.StatusCode);
        }
    }

}
