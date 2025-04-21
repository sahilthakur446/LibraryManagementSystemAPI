using LibraryManagementSystem.DTOs.User;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Services.Interfaces
{
    public interface IAuthService
    {
        Task<string> Login(string username, string password);
        Task<CreateUserDTO> CreateNewUser(CreateUserDTO user);
    }
}
