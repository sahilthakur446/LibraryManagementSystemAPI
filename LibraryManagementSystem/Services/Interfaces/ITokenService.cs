using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateJWT(User user);
    }
}
