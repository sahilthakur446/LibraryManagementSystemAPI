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

        [HttpPost("bulk")]
        [ProducesResponseType(typeof(ApiResponse<IList<BookDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<IList<BookDTO>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<IList<BookDTO>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddBooks([FromBody] List<BookRequestDTO> bookDTOs)
        {
            try
            {
                var result = await bookService.AddBooks(bookDTOs);
                return Ok(ApiResponse<IList<BookDTO>>.Success(result, "Books added successfully"));
            }
            catch (BusinessExceptions ex)
            {
                return StatusCode(ex.StatusCode, ApiResponse<IList<BookDTO>>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while adding books");
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<IList<BookDTO>>.Fail("An unexpected error occurred"));
            }
        }
        [HttpGet("with-copies")]
        [ProducesResponseType(typeof(ApiResponse<IList<BookDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<IList<BookDTO>>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<IList<BookDTO>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllBooksWithCopies()
        {
            try
            {
                var result = await bookService.GetAllBooksWithCopies();
                return Ok(ApiResponse<IList<BookDTO>>.Success(result, "Books with copies fetched successfully"));
            }
            catch (BusinessExceptions ex)
            {
                return StatusCode(ex.StatusCode, ApiResponse<IList<BookDTO>>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while fetching books with copies");
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<IList<BookDTO>>.Fail("An unexpected error occurred"));
            }
        }
        [HttpGet("{id}/with-copies")]
        [ProducesResponseType(typeof(ApiResponse<BookDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<BookDTO>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<BookDTO>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBookWithCopies(int id)
        {
            try
            {
                var result = await bookService.GetBookWithCopies(id);
                return Ok(ApiResponse<BookDTO>.Success(result, $"Book with ID {id} and its copies fetched successfully"));
            }
            catch (BusinessExceptions ex)
            {
                return StatusCode(ex.StatusCode, ApiResponse<BookDTO>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"An unexpected error occurred while fetching book with ID {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<BookDTO>.Fail("An unexpected error occurred"));
            }
        }

        [HttpGet("bookCopy/{bookCopyId}")]
        [ProducesResponseType(typeof(ApiResponse<BookCopyResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBookCopyById([FromRoute]int bookCopyId)
        {
            try
            {
                var result = await bookService.GetBookCopyById(bookCopyId);
                return Ok(ApiResponse<BookCopyResponseDTO>.Success(result,"Book copy retrieved successfully."));
            }
            catch (BusinessExceptions ex)
            {
                return StatusCode(ex.StatusCode, ApiResponse<bool>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while adding a book copy");
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<bool>.Fail("An unexpected error occurred"));
            }

        }

        [HttpPost("copy")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddBookCopy([FromBody] BookCopyRequestDTO bookCopyDTO)
        {
            try
            {
                var result = await bookService.AddBookCopy(bookCopyDTO);
                return Ok(ApiResponse<bool>.Success(result, "Book copy added successfully"));
            }
            catch (BusinessExceptions ex)
            {
                return StatusCode(ex.StatusCode, ApiResponse<bool>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while adding a book copy");
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<bool>.Fail("An unexpected error occurred"));
            }
        }
        [HttpPut("copy/{bookCopyId}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateBookCopy(int bookCopyId, [FromBody] BookCopyRequestDTO bookCopyDto)
        {
            try
            {
                var result = await bookService.UpdateBookCopy(bookCopyId, bookCopyDto);
                return Ok(ApiResponse<bool>.Success(result, $"Book copy with ID {bookCopyId} updated successfully"));
            }
            catch (BusinessExceptions ex)
            {
                return StatusCode(ex.StatusCode, ApiResponse<bool>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"An unexpected error occurred while updating book copy with ID {bookCopyId}");
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<bool>.Fail("An unexpected error occurred"));
            }
        }
        [HttpDelete("copy/{bookCopyId}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteBookCopy(int bookCopyId)
        {
            try
            {
                var result = await bookService.DeleteBookCopy(bookCopyId);
                return Ok(ApiResponse<bool>.Success(result, $"Book copy with ID {bookCopyId} deleted successfully"));
            }
            catch (BusinessExceptions ex)
            {
                return StatusCode(ex.StatusCode, ApiResponse<bool>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"An unexpected error occurred while deleting book copy with ID {bookCopyId}");
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<bool>.Fail("An unexpected error occurred"));
            }
        }

    }
}
