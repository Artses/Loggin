using Api_Loggin.DTOs;
using Api_Loggin.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api_Loggin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogController : ControllerBase
    {
        private readonly IWebSocketService _websocketService;

        public LogController(IWebSocketService websocketService)
        {
            _websocketService = websocketService;
        }

        [HttpPost("GetLog")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> SendLog([FromBody] WriteMessageDto dto)
        {
            await _websocketService.ConnectAsync();
            await _websocketService.MessageAsync(dto);
            return Ok(new { message = "Log sent!" });
        }
    }
}
