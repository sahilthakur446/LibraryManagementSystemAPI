using LibraryManagementSystem.DTOs.User;
using LibraryManagementSystem.Exceptions;
using LibraryManagementSystem.ApiResponse;
using LibraryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult> GetUsers()
        {
            try
            {
                var users = await _userService.GetAllUsers();
                if (!users.Any())
                {
                    return NotFound(ApiResponse<List<UserDTO>>.Fail("No users found"));
                }
                return Ok(ApiResponse<List<UserDTO>>.Success(users, ""));
            }
            catch (BusinessExceptions ex)
            {
                return BadRequest(ApiResponse<List<UserDTO>>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<List<UserDTO>>.Fail("An unexpected error occurred"));
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult> GetUserById(int id)
        {
            try
            {
                var user = await _userService.GetUserById(id);
                if (user == null)
                {
                    return NotFound(ApiResponse<UserDTO>.Fail("No user found"));
                }
                return Ok(ApiResponse<UserDTO>.Success(user, ""));
            }
            catch (BusinessExceptions ex)
            {
                return BadRequest(ApiResponse<UserDTO>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<UserDTO>.Fail("An unexpected error occurred"));
            }
        }

        [HttpGet("by-email/{email}")]
        public async Task<ActionResult> GetUserByEmail(string email)
        {
            try
            {
                var user = await _userService.GetUserByEmail(email);
                if (user == null)
                {
                    return NotFound(ApiResponse<UserDTO>.Fail("No user found"));
                }
                return Ok(ApiResponse<UserDTO>.Success(user, ""));
            }
            catch (BusinessExceptions ex)
            {
                return BadRequest(ApiResponse<UserDTO>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<UserDTO>.Fail("An unexpected error occurred"));
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> UpdateUser(int id, [FromBody] UpdateUserDTO user)
        {
            try
            {
                if (user == null)
                {
                    return BadRequest(ApiResponse<UserDTO>.Fail("Invalid user data"));
                }

                var updatedUser = await _userService.UpdateUserDetails(id, user);
                if (updatedUser == null)
                {
                    return NotFound(ApiResponse<UserDTO>.Fail("User not found or update failed"));
                }

                return Ok(ApiResponse<UserDTO>.Success(updatedUser, "User updated successfully"));
            }
            catch (BusinessExceptions ex)
            {
                return BadRequest(ApiResponse<UserDTO>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<UserDTO>.Fail("An unexpected error occurred"));
            }
        }
    }
}
