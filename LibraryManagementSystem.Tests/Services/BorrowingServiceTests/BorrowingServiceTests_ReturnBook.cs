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
    public class BorrowingServiceTests_ReturnBook
    {
        [Fact]
        public async Task ReturnBook_ShouldThrow_WhenReturnedBookIsNull()
        {
            // Arrange
            var borrowingRepositoryMock = new Mock<IBorrowingRepository>();
            var emailServiceMock = new Mock<IEmailService>();
            var loggerMock = new Mock<ILogger<BorrowingService>>();
            var borrowingService = new BorrowingService(borrowingRepositoryMock.Object, emailServiceMock.Object, loggerMock.Object);

            int borrowingId = 1;

            borrowingRepositoryMock.Setup(repo => repo.ReturnBookAsync(borrowingId))
                .ReturnsAsync((BorrowedBook)null); 

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessExceptions>(() => borrowingService.ReturnBookAsync(borrowingId));
            Assert.Equal("Borrowing Record Not Found.", exception.Message);
            Assert.Equal(StatusCodes.Status404NotFound, exception.StatusCode);
        }


    }

}
