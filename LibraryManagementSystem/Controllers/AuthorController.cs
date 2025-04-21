using LibraryManagementSystem.ApiResponse;
using LibraryManagementSystem.DTOs.Author;
using LibraryManagementSystem.DTOs.Book;
using LibraryManagementSystem.Exceptions;
using LibraryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers
{
    [Route("api/authors")]
    [ApiController]
    public class AuthorController : ControllerBase
    {
        private readonly IAuthorService _authorService;

        public AuthorController(IAuthorService authorService)
        {
            _authorService = authorService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAuthors()
        {
            try
            {
                var authors = await _authorService.GetAllAuthors();
                if (authors == null || !authors.Any())
                {
                    return NotFound(ApiResponse<IEnumerable<AuthorDTO>>.Fail("No authors found."));
                }
                return Ok(ApiResponse<IEnumerable<AuthorDTO>>.Success(authors, "Authors retrieved successfully."));
            }
            catch (BusinessExceptions ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail("Internal server error."));
            }
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAuthorById(int id)
        {
            try
            {
                var author = await _authorService.GetAuthorById(id);
                if (author == null)
                {
                    return NotFound(ApiResponse<AuthorDTO>.Fail($"No author found with ID: {id}"));
                }
                return Ok(ApiResponse<AuthorDTO>.Success(author, "Author retrieved successfully."));
            }
            catch (BusinessExceptions ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail("Internal server error."));
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddAuthor([FromBody] AuthorRequestDTO author)
        {
            try
            {
                bool isInserted = await _authorService.AddAuthor(author);
                if (!isInserted)
                {
                    return BadRequest(ApiResponse<string>.Fail("Error adding author."));
                }
                return Ok(ApiResponse<string>.Success("Author created successfully."));
            }
            catch (BusinessExceptions ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail("Internal server error."));
            }
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAuthor(int id, [FromBody] AuthorRequestDTO author)
        {
            try
            {
                bool isUpdated = await _authorService.UpdateAuthor(id, author);
                if (!isUpdated)
                {
                    return NotFound(ApiResponse<string>.Fail($"No author found with ID: {id}"));
                }
                return Ok(ApiResponse<string>.Success("Author updated successfully."));
            }
            catch (BusinessExceptions ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail("Internal server error."));
            }
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAuthor(int id)
        {
            try
            {
                bool isDeleted = await _authorService.DeleteAuthor(id);
                if (!isDeleted)
                {
                    return NotFound(ApiResponse<string>.Fail($"No author found with ID: {id}"));
                }
                return Ok(ApiResponse<string>.Success("Author deleted successfully."));
            }
            catch (BusinessExceptions ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail("Internal server error."));
            }
        }

        [HttpGet("{id:int}/books")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBooksByAuthor(int id)
        {
            try
            {
                var books = await _authorService.GetBooksByAuthor(id);
                if (books == null || !books.Any())
                {
                    return NotFound(ApiResponse<List<BookDTO>>.Fail($"No books found for author with ID: {id}"));
                }
                return Ok(ApiResponse<List<BookDTO>>.Success(books, "Books retrieved successfully."));
            }
            catch (BusinessExceptions ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail("Internal server error."));
            }
        }
        [HttpDelete("{id:int}/books")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteAllBooksByAuthor(int id)
        {
            try
            {
                bool result = await _authorService.DeleteBooksByAuthor(id);
                if (!result)
                {
                    return NotFound(ApiResponse<bool>.Fail($"No books found or already deleted in author ID: {id}"));
                }

                return Ok(ApiResponse<bool>.Success(true, "All books deleted from the author successfully."));
            }
            catch (BusinessExceptions ex)
            {
                return StatusCode(ex.StatusCode, ApiResponse<bool>.Fail(ex.Message));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail("Internal server error."));
            }
        }
    }
}