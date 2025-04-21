using LibraryManagementSystem.DTOs.User;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Repositories.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetUserByEmail(string email); 
        Task<bool> EmailAlreadyExist(string email);
    }
}
