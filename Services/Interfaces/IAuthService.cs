using Api_Loggin.DTOs;

namespace Api_Loggin.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthUserResponseDto> RegisterUserAsync(RegisterUserDto registerDto);
        Task<AuthUserResponseDto> LoginUserAsync(LoginUserDto loginDto);
    }
}
