using Api_Loggin.Data;
using Api_Loggin.DTOs;
using Api_Loggin.Models;
using Api_Loggin.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api_Loggin.Services
{
    public class AuthServices(AppDbContext db, IConfiguration config) : IAuthService
    {
        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            if (await db.Users.AnyAsync(u => u.Email == dto.Email))
                return null;

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            return new AuthResponseDto(GenerateToken(user), user.Name, user.Email, user.Role);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return null;

            return new AuthResponseDto(GenerateToken(user), user.Name, user.Email, user.Role);
        }

        public string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(config["Jwt:ExpiresInMinutes"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}