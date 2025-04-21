using LibraryManagementSystem.DTOs.Category;
using LibraryManagementSystem.DTOs.Book;
using LibraryManagementSystem.Exceptions;
using LibraryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.ApiResponse;

namespace LibraryManagementSystem.Controllers
{
    [Route("api/categories")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _CategoryService;

        public CategoryController(ICategoryService CategoryService)
        {
            _CategoryService = CategoryService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await (_CategoryService.GetAllCategories());
                if (categories == null || !categories.Any())
                {
                    return NotFound(ApiResponse<IEnumerable<CategoryDTO>>.Fail("No categories found."));
                }

                return Ok(ApiResponse<IEnumerable<CategoryDTO>>.Success(categories, "Categories fetched successfully."));
            }
            catch (BusinessExceptions ex)
            {
                return StatusCode(ex.StatusCode, ApiResponse<string>.Fail(ex.Message));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail("Internal server error."));
            }
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            try
            {
                var category = await _CategoryService.GetCategoryById(id);
                if (category == null)
                {
                    return NotFound(ApiResponse<CategoryDTO>.Fail($"No category found with ID: {id}"));
                }

                return Ok(ApiResponse<CategoryDTO>.Success(category, "Category fetched successfully."));
            }
            catch (BusinessExceptions ex)
            {
                return StatusCode(ex.StatusCode, ApiResponse<string>.Fail(ex.Message));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail("Internal server error."));
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> AddCategory([FromBody] CategoryRequestDTO category)
        {
            try
            {
                bool isInserted = await _CategoryService.AddCategory(category);
                if (!isInserted)
                {
                    return BadRequest(ApiResponse<bool>.Fail("Failed to create category."));
                }

                return Ok(ApiResponse<bool>.Success(true, "Category created successfully."));
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

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryRequestDTO category)
        {
            try
            {
                bool isUpdated = await _CategoryService.UpdateCategory(id, category);
                if (!isUpdated)
                {
                    return NotFound(ApiResponse<bool>.Fail($"No category found with ID: {id}"));
                }

                return Ok(ApiResponse<bool>.Success(true, "Category updated successfully."));
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

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                bool isDeleted = await _CategoryService.DeleteCategory(id);
                if (!isDeleted)
                {
                    return NotFound(ApiResponse<bool>.Fail($"No category found with ID: {id}"));
                }

                return Ok(ApiResponse<bool>.Success(true, "Category deleted successfully."));
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

        [HttpGet("{id:int}/books")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBooksByCategory(int id)
        {
            try
            {
                var books = await _CategoryService.GetBooksByCategory(id);
                if (books == null || !books.Any())
                {
                    return NotFound(ApiResponse<List<BookDTO>>.Fail($"No books found for category with ID: {id}"));
                }

                return Ok(ApiResponse<List<BookDTO>>.Success(books, "Books fetched successfully."));
            }
            catch (BusinessExceptions ex)
            {
                return StatusCode(ex.StatusCode, ApiResponse<string>.Fail(ex.Message));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail("Internal server error."));
            }
        }

        [HttpDelete("{id:int}/books")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteAllBooksInCategory(int id)
        {
            try
            {
                bool result = await _CategoryService.DeleteBooksByCategory(id);
                if (!result)
                {
                    return NotFound(ApiResponse<bool>.Fail($"No books found or already deleted in category ID: {id}"));
                }

                return Ok(ApiResponse<bool>.Success(true, "All books deleted from the category successfully."));
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
