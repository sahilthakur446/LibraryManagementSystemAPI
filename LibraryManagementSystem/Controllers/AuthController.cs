using LibraryManagementSystem.DTOs.User;
using LibraryManagementSystem.Exceptions;
using LibraryManagementSystem.ApiResponse;
using LibraryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginUserDTO loginDto)
    {
        try
        {
            var token = await _authService.Login(loginDto.Email, loginDto.Password);
            return Ok(ApiResponse<string>.Success(token, "Login successful"));
        }
        catch (BusinessExceptions ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<string>.Fail(ex.Message));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<string>.Fail("An unexpected error occurred"));
        }
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] CreateUserDTO user)
    {
        try
        {
            var createdUser = await _authService.CreateNewUser(user);
            return CreatedAtAction(nameof(Register),
                ApiResponse<CreateUserDTO>.Success(createdUser, "User registered successfully"));
        }
        catch (BusinessExceptions ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<string>.Fail(ex.Message));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<string>.Fail("An unexpected error occurred"));
        }
    }
}