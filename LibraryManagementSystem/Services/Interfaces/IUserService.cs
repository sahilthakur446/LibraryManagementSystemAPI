using LibraryManagementSystem.DTOs.User;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<UserDTO>> GetAllUsers();
        Task<UserDTO?> GetUserById(int id);
        Task<UserDTO?> GetUserByEmail(string email);
        Task<UserDTO?> UpdateUserDetails(int id, UpdateUserDTO user);
    }
}
