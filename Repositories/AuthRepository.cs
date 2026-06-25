using Api_Loggin.Data;
using Api_Loggin.Models;
using Api_Loggin.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api_Loggin.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AppDbContext _context;
    
        public AuthRepository(AppDbContext dbContext)
        {
            _context = dbContext;
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }
    }
}
