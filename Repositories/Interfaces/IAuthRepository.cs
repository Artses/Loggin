using Api_Loggin.Models;

namespace Api_Loggin.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        Task<User> GetUserByEmailAsync(string email);
        Task AddUserAsync(User user);
    }
}
