using LibraryManagementSystem.DTOs.User;
using LibraryManagementSystem.Enums;
using LibraryManagementSystem.Exceptions;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories.Interfaces;
using LibraryManagementSystem.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace LibraryManagementSystem.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(ILogger<UserService> logger, IUserRepository userRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
        }

        public async Task<List<UserDTO>> GetAllUsers()
        {
            try
            {
                var users = await _userRepository.GetAll();
                return users.Select(user => new UserDTO
                {
                    UserId = user.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Role = ((RolesEnum)user.RoleId).ToString()
                }).ToList();
            }
            catch (RepositoryException ex)
            {
                _logger.LogError(ex, "Repository error while retrieving all users");
                throw new BusinessExceptions("Error retrieving all users", StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred in GetAllUsers");
                throw;
            }
        }

        public async Task<UserDTO?> GetUserById(int id)
        {
            try
            {
                var user = await _userRepository.GetById(id);
                if (user == null) return null;

                return new UserDTO
                {
                    UserId = user.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Role = ((RolesEnum)user.RoleId).ToString()
                };
            }
            catch (RepositoryException ex)
            {
                _logger.LogError(ex, $"Repository error while retrieving user with ID {id}");
                throw new BusinessExceptions("Error retrieving user by ID", StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error in GetUserById for user ID {id}");
                throw;
            }
        }

        public async Task<UserDTO?> GetUserByEmail(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    throw new BusinessExceptions("Please provide a valid email", StatusCodes.Status400BadRequest);

                email = email.Trim();
                var user = await _userRepository.GetUserByEmail(email);

                if (user == null) return null;

                return new UserDTO
                {
                    UserId = user.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Role = ((RolesEnum)user.RoleId).ToString()
                };
            }
            catch (RepositoryException ex)
            {
                _logger.LogError(ex, $"Repository error while retrieving user with email: {email}");
                throw new BusinessExceptions("Error retrieving user by email", StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error in GetUserByEmail for email {email}");
                throw;
            }
        }

        public async Task<UserDTO?> UpdateUserDetails(int id, UpdateUserDTO user)
        {
            try
            {
                if (user == null)
                    throw new ArgumentNullException(nameof(user), "User details cannot be null");

                var existingUser = await _userRepository.GetById(id);
                if (existingUser == null)
                    throw new BusinessExceptions($"User with ID {id} not found", StatusCodes.Status404NotFound);

                // Check if the email already exists for another user
                if (await _userRepository.EmailAlreadyExist(user.Email))
                    throw new BusinessExceptions($"The email '{user.Email}' is already associated with another account.", StatusCodes.Status409Conflict);

                // Update user details
                existingUser.Email = user.Email;
                existingUser.FirstName = user.FirstName;
                existingUser.LastName = user.LastName;

                var isUpdated = await _userRepository.Update(existingUser);
                if (!isUpdated)
                {
                    _logger.LogWarning($"Failed to update user with ID {id}");
                    throw new BusinessExceptions($"Failed to update user with ID {id}", StatusCodes.Status500InternalServerError);
                }

                return new UserDTO
                {
                    UserId = existingUser.UserId,
                    FirstName = existingUser.FirstName,
                    LastName = existingUser.LastName,
                    Email = existingUser.Email,
                    Role = ((RolesEnum)existingUser.RoleId).ToString()
                };
            }
            catch (RepositoryException ex)
            {
                _logger.LogError(ex, $"Repository error while updating user with ID {id}");
                throw new BusinessExceptions("Error updating user details", StatusCodes.Status500InternalServerError);
            }
            catch (BusinessExceptions ex)
            {
                _logger.LogError(ex, $"Business error while updating user with ID {id}");
                throw; // Re-throw the BusinessExceptions as it already has status code
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error in UpdateUserDetails for user ID {id}");
                throw;
            }
        }
    }
}