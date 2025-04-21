using LibraryManagementSystem.Repositories.Interfaces;
using LibraryManagementSystem.Services.Interfaces;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.DTOs.User;
using LibraryManagementSystem.Exceptions;
namespace LibraryManagementSystem.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUserRepository userRepository, ITokenService tokenService, ILogger<AuthService> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("Login failed: Username or password is empty.");
                throw new BusinessExceptions("Username and password are required.",StatusCodes.Status400BadRequest);
            }

            var user = await _userRepository.GetUserByEmail(username);
            if (user?.Email == null)
            {
                _logger.LogWarning("Login failed: Email not found - {Username}", username);
                throw new BusinessExceptions("Email not found", StatusCodes.Status404NotFound);
            }

            if (!VerifyPassword(password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed: Incorrect password for {Username}", username);
                throw new BusinessExceptions("Wrong password", StatusCodes.Status400BadRequest);
            }

            var token = _tokenService.GenerateJWT(user);
            _logger.LogInformation("JWT token generated successfully for {Username}", username);
            return token;
        }

        public async Task<CreateUserDTO> CreateNewUser(CreateUserDTO userDto)
        {
            if (userDto == null)
                throw new ArgumentNullException(nameof(userDto));

            if (string.IsNullOrWhiteSpace(userDto.Email) || string.IsNullOrWhiteSpace(userDto.Password))
            {
                _logger.LogWarning("User registration failed: Missing email or password.");
                throw new BusinessExceptions("Email and password are required", StatusCodes.Status400BadRequest);
            }

            if (await _userRepository.EmailAlreadyExist(userDto.Email))
            {
                _logger.LogWarning("User registration failed: Email already exists - {Email}", userDto.Email);
                throw new BusinessExceptions("Email already exists", StatusCodes.Status409Conflict);
            }

            userDto.Password = HashPassword(userDto.Password);
            var user = new User
            {
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email,
                PasswordHash = userDto.Password,
                RoleId = userDto.RoleId
            };

            var isInserted = await _userRepository.Insert(user);
            if (!isInserted)
            {
                _logger.LogError("User registration failed: Unable to insert user - {Email}", userDto.Email);
                throw new BusinessExceptions("Failed to create user", StatusCodes.Status500InternalServerError);
            }

            _logger.LogInformation("User registered successfully: {Email}", userDto.Email);
            return userDto;
        }

        private string HashPassword(string password)
        {
            try
            {
                return BCrypt.Net.BCrypt.HashPassword(password);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error hashing password");
                throw new BusinessExceptions("Error processing password", StatusCodes.Status500InternalServerError);
            }
        }

        private bool VerifyPassword(string enteredPassword, string storedPassword)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(enteredPassword, storedPassword);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying password");
                throw new BusinessExceptions("Error verifying password", StatusCodes.Status500InternalServerError);
            }
        }
    }
}