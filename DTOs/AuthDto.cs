namespace Api_Loggin.DTOs
{
public record RegisterUserDto(string Name, string Email, string Password);
public record LoginUserDto(string Email, string Password);
public record AuthUserResponseDto(string Token, string Name, string Email, string Role);
}