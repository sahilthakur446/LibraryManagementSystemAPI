using LibraryManagementSystem.ApiResponse;
using LibraryManagementSystem.DTOs.Borrowing;
using LibraryManagementSystem.Exceptions;
using LibraryManagementSystem.Services.Implementations;
using LibraryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers
{
    [Route("api/borrowing")]
    [ApiController]
    public class BorrowingController : ControllerBase
    {
        private readonly IBorrowingService borrowingService;
        private readonly ILogger<BorrowingController> logger;

        public BorrowingController(IBorrowingService borrowingService, ILogger<BorrowingController> logger)
        {
            this.borrowingService = borrowingService;
            this.logger = logger;
        }

        [HttpPost("borrow")]
        [ProducesResponseType(typeof(ApiResponse<BorrowingResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<BorrowingResponseDTO>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<BorrowingResponseDTO>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<BorrowingResponseDTO>>> BorrowBook(int bookId, int userId, int bookCopyId = 0)
        {
            try
            {
                var result = await borrowingService.BorrowBookAsync(bookId, userId);
                return Ok(ApiResponse<BorrowingResponseDTO>.Success(result, "Book borrowed successfully"));
            }
            catch (BusinessExceptions ex)
            {
                logger.LogWarning(ex, "Business error while borrowing book");
                return StatusCode(ex.StatusCode, ApiResponse<BorrowingResponseDTO>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error in BorrowBook");
                return StatusCode(500, ApiResponse<BorrowingResponseDTO>.Fail("An unexpected error occurred"));
            }
        }

        [HttpPost("return")]
        [ProducesResponseType(typeof(ApiResponse<BorrowingResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<BorrowingResponseDTO>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<BorrowingResponseDTO>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<BorrowingResponseDTO>>> ReturnBook(int borrowingId)
        {
            try
            {
                var result = await borrowingService.ReturnBookAsync(borrowingId);
                return Ok(ApiResponse<BorrowingResponseDTO>.Success(result, "Book returned successfully"));
            }
            catch (BusinessExceptions ex)
            {
                logger.LogWarning(ex, "Business error while returning book");
                return StatusCode(ex.StatusCode, ApiResponse<BorrowingResponseDTO>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error in ReturnBook");
                return StatusCode(500, ApiResponse<BorrowingResponseDTO>.Fail("An unexpected error occurred"));
            }
        }

        [HttpGet("available-copies/{bookId}")]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<int>>> GetAvailableBookCopies(int bookId)
        {
            try
            {
                var count = await borrowingService.GetAvailableBookCopiesAsync(bookId);
                return Ok(ApiResponse<int>.Success(count, "Available copies fetched"));
            }
            catch (BusinessExceptions ex)
            {
                logger.LogWarning(ex, "Business error while getting available copies");
                return StatusCode(ex.StatusCode, ApiResponse<int>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error in GetAvailableBookCopies");
                return StatusCode(500, ApiResponse<int>.Fail("An unexpected error occurred"));
            }
        }

        [HttpGet("borrowed/user/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<IList<BorrowingResponseDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<IList<BorrowingResponseDTO>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<IList<BorrowingResponseDTO>>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IList<BorrowingResponseDTO>>>> GetBorrowedBooksByUserId(int userId)
        {
            try
            {
                var result = await borrowingService.GetBorrowedBooksByUserIdAsync(userId);
                return Ok(ApiResponse<IList<BorrowingResponseDTO>>.Success(result, "Borrowed books fetched"));
            }
            catch (BusinessExceptions ex)
            {
                logger.LogWarning(ex, "Business error while getting borrowed books by user");
                return StatusCode(ex.StatusCode, ApiResponse<IList<BorrowingResponseDTO>>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error in GetBorrowedBooksByUserId");
                return StatusCode(500, ApiResponse<IList<BorrowingResponseDTO>>.Fail("An unexpected error occurred"));
            }
        }

        [HttpGet("borrowed/all")]
        [ProducesResponseType(typeof(ApiResponse<IList<BorrowingResponseDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<IList<BorrowingResponseDTO>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<IList<BorrowingResponseDTO>>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IList<BorrowingResponseDTO>>>> GetAllBorrowedBooks()
        {
            try
            {
                var result = await borrowingService.GetAllBorrowedBooksAsync();
                return Ok(ApiResponse<IList<BorrowingResponseDTO>>.Success(result, "All borrowed books fetched"));
            }
            catch (BusinessExceptions ex)
            {
                logger.LogWarning(ex, "Business error while getting all borrowed books");
                return StatusCode(ex.StatusCode, ApiResponse<IList<BorrowingResponseDTO>>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error in GetAllBorrowedBooks");
                return StatusCode(500, ApiResponse<IList<BorrowingResponseDTO>>.Fail("An unexpected error occurred"));
            }
        }

        [HttpGet("borrowed/overdue")]
        [ProducesResponseType(typeof(ApiResponse<IList<BorrowingResponseDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<IList<BorrowingResponseDTO>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<IList<BorrowingResponseDTO>>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IList<BorrowingResponseDTO>>>> GetAllOverDueBooks()
        {
            try
            {
                var result = await borrowingService.GetAllOverDueBooksAsync();
                return Ok(ApiResponse<IList<BorrowingResponseDTO>>.Success(result, "Overdue books fetched"));
            }
            catch (BusinessExceptions ex)
            {
                logger.LogWarning(ex, "Business error while getting overdue books");
                return StatusCode(ex.StatusCode, ApiResponse<IList<BorrowingResponseDTO>>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error in GetAllOverDueBooks");
                return StatusCode(500, ApiResponse<IList<BorrowingResponseDTO>>.Fail("An unexpected error occurred"));
            }
        }

        [HttpGet("fine/user/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<int>>> GetRemainingFineByUserId(int userId)
        {
            try
            {
                var result = await borrowingService.GetRemainingFineByUserIdAsync(userId);
                return Ok(ApiResponse<int>.Success(result, "Remaining fine fetched"));
            }
            catch (BusinessExceptions ex)
            {
                logger.LogWarning(ex, "Business error while getting remaining fine");
                return StatusCode(ex.StatusCode, ApiResponse<int>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error in GetRemainingFineByUserId");
                return StatusCode(500, ApiResponse<int>.Fail("An unexpected error occurred"));
            }
        }

        [HttpGet("due-tomorrow-email")]
        [ProducesResponseType(typeof(IList<BorrowedBookEmailDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBorrowedBooksDueTomorrowForEmail()
        {
            try
            {
                var borrowings = await borrowingService.GetBorrowedBooksDueTomorrowForEmailAsync();
                return Ok(borrowings);
            }
            catch (BusinessExceptions ex)
            {
                logger.LogError(ex, "Failed to get borrowings due tomorrow.");
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("due-email")]
        [ProducesResponseType(typeof(IList<BorrowedBookEmailDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllOverDueBooksForEmail()
        {
            try
            {
                var borrowings = await borrowingService.GetAllOverDueBooksForEmailAsync();
                return Ok(borrowings);
            }
            catch (BusinessExceptions ex)
            {
                logger.LogError(ex, "Failed to get borrowings due tomorrow.");
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
