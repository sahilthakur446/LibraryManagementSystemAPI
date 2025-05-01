using LibraryManagementSystem.ApiResponse;
using LibraryManagementSystem.DTOs.Book;
using LibraryManagementSystem.Exceptions;
using LibraryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers
{
    [Route("api/books")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBookService bookService;
        private readonly ILogger<BooksController> logger;

        public BooksController(IBookService bookService, ILogger<BooksController> logger)
        {
            this.bookService = bookService;
            this.logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<BookDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllBooks()
        {
            try
            {
                var books = await bookService.GetAllBooks();
                return Ok(ApiResponse<IList<BookDTO>>.Success(books, "All books retrieved successfully"));
            }
            catch (BusinessExceptions ex)
            {
                logger.LogError(ex, "Controller: An error occurred while fetching all books");
                return StatusCode(ex.StatusCode, ApiResponse<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while fetching all books");
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail("An unexpected error occurred"));
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<BookDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBook(int id)
        {
            try
            {
                var book = await bookService.GetBookById(id);
                return Ok(ApiResponse<BookDTO>.Success(book, "Book retrieved successfully"));
            }
            catch (BusinessExceptions ex)
            {
                return StatusCode(ex.StatusCode, ApiResponse<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"An error occurred while fetching book with ID {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail("An unexpected error occurred"));
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<bool?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateBook([FromBody] BookRequestDTO bookRequestDTO)
        {
            try
            {
                var createdBook = await bookService.AddBook(bookRequestDTO);
                return StatusCode(StatusCodes.Status201Created, ApiResponse<bool?>.Success(null, "Book created successfully"));
            }
            catch (BusinessExceptions ex)
            {
                return StatusCode(ex.StatusCode, ApiResponse<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while creating a book");
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail("An unexpected error occurred"));
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] BookRequestDTO bookRequestDTO)
        {
            try
            {
                var updatedBook = await bookService.UpdateBook(id, bookRequestDTO);
                return Ok(ApiResponse<bool?>.Success(null, "Book updated successfully"));
            }
            catch (BusinessExceptions ex)
            {
                return StatusCode(ex.StatusCode, ApiResponse<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"An error occurred while updating book with ID {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail("An unexpected error occurred"));
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteBook(int id)
        {
            try
            {
                await bookService.DeleteBook(id);
                return Ok(ApiResponse<string>.Success(null, $"Book with ID {id} deleted successfully"));
            }
            catch (BusinessExceptions ex)
            {
                return StatusCode(ex.StatusCode, ApiResponse<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"An error occurred while deleting book with ID {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail("An unexpected error occurred"));
            }
        }
    }
}
