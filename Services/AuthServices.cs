using Api_Loggin.Data;
using Api_Loggin.DTOs;
using Api_Loggin.Models;
using Api_Loggin.Repositories.Interfaces;
using Api_Loggin.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api_Loggin.Services
{
    public class AuthServices : IAuthService
    {

        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;

        public AuthServices(IAuthRepository repo, IConfiguration config)
        {
            _repo = repo;
            _config = config;
        }
        public async Task<AuthUserResponseDto> RegisterUserAsync(RegisterUserDto dto)
        {
            if (await _repo.GetUserByEmailAsync(dto.Email) != null)
                return null;

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            };

            await _repo.AddUserAsync(user);

            return new AuthUserResponseDto(GenerateToken(user), user.Name, user.Email, user.Role);
        }

        public async Task<AuthUserResponseDto> LoginUserAsync(LoginUserDto dto)
        {
            var user = await _repo.GetUserByEmailAsync(dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return null;

            return new AuthUserResponseDto(GenerateToken(user), user.Name, user.Email, user.Role);
        }

        public string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT_KEY"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpiresInMinutes"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}