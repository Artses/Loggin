using Api_Loggin.DTOs;
using Api_Loggin.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api_Loggin.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService authService) : ControllerBase
    {

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await authService.RegisterAsync(dto);
            if (result is null)
            {
                return Conflict(new { message = "Invalid Credentials" });
            }
            return CreatedAtAction(nameof(Register), result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await authService.LoginAsync(dto);

            if (result is null)
            {
                return Unauthorized(new { message = "Invalid Credentials" });
            }
            return Ok(result);
        }

        [HttpGet("hi")]
        [Authorize(Roles = "User")]
        public IActionResult Hi() {
            return Ok(new { message = "Hello this route is only for testing purpose" });
        }
    }
}