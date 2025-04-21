using LibraryManagementSystem.Exceptions;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace LibraryManagementSystem.Repositories.EF
{
    public class UserRepositoryEF : IUserRepository
    {
        private readonly LibraryDbContext _dbContext;
        private readonly ILogger<UserRepositoryEF> _logger;

        public UserRepositoryEF(LibraryDbContext dbContext, ILogger<UserRepositoryEF> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            try
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);

                if (user is null)
                {
                    _logger.LogInformation("No user found with email: {Email}", email);
                }
                return user;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error while retrieving user by email: {Email}", email);
                throw new RepositoryException("A database error occurred while retrieving the user by email.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving user by email: {Email}", email);
                throw new RepositoryException("An unexpected error occurred while retrieving the user by email.", ex);
            }
        }

        public async Task<bool> EmailAlreadyExist(string email)
        {
            try
            {
                return await _dbContext.Users.AnyAsync(user => user.Email == email);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error while checking email existence: {Email}", email);
                throw new RepositoryException("A database error occurred while checking email existence.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error checking email existence: {Email}", email);
                throw new RepositoryException("An unexpected error occurred while checking email existence.", ex);
            }
        }

        public async Task<IEnumerable<User?>> GetAll()
        {
            try
            {
                return await _dbContext.Users.ToListAsync();
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error while retrieving all users.");
                throw new RepositoryException("A database error occurred while retrieving users.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving all users.");
                throw new RepositoryException("An unexpected error occurred while retrieving users.", ex);
            }
        }

        public async Task<User?> GetById(int userId)
        {
            try
            {
                return await _dbContext.Users.FirstOrDefaultAsync(user => user.UserId == userId);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error while retrieving user by ID: {UserId}", userId);
                throw new RepositoryException("A database error occurred while retrieving the user.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving user by ID: {UserId}", userId);
                throw new RepositoryException("An unexpected error occurred while retrieving the user.", ex);
            }
        }

        public async Task<bool> Insert(User user)
        {
            try
            {
                await _dbContext.Users.AddAsync(user);
                return await _dbContext.SaveChangesAsync() > 0;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update error while inserting user: {@User}", user);
                throw new RepositoryException("A database update error occurred while inserting the user.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error inserting user: {@User}", user);
                throw new RepositoryException("An unexpected error occurred while inserting the user.", ex);
            }
        }

        public async Task<bool> Update(User user)
        {
            try
            {
                _dbContext.Users.Update(user);
                return await _dbContext.SaveChangesAsync() > 0;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update error while updating user: {@User}", user);
                throw new RepositoryException("A database update error occurred while updating the user.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating user: {@User}", user);
                throw new RepositoryException("An unexpected error occurred while updating the user.", ex);
            }
        }

        public async Task<bool> Delete(int userId)
        {
            try
            {
                int affectedRows = await _dbContext.Users.Where(user => user.UserId == userId).ExecuteDeleteAsync();
                return affectedRows > 0;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update error while deleting user with ID: {UserId}", userId);
                throw new RepositoryException("A database update error occurred while deleting the user.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting user with ID: {UserId}", userId);
                throw new RepositoryException("An unexpected error occurred while deleting the user.", ex);
            }
        }
    }
}
