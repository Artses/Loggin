using Api_Loggin.DTOs;
using Api_Loggin.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api_Loggin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User")]
    public class LogController : ControllerBase
    {
        private readonly ILogService _service;

        public LogController(ILogService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> AddLog([FromBody] RegisterLogDto dto)
        {

            var result = await _service.RegisterLogAsync(dto);
            if (result is null)
            {
                return BadRequest(new { message = "Failed to register Log" });
            }

            return CreatedAtAction(nameof(GetLogById), new { id = result.Id }, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLogs()
        {
            var Logs = await _service.GetAllLogsAsync();
            return Ok(Logs);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetLogById(Guid id)
        {
            var Log = await _service.GetLogAsync(id);
            if (Log is null)
            {
                return NotFound(new { message = "Log not found" });
            }

            return Ok(Log);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateLog([FromBody] UpdateLogDto dto)
        {


            var result = await _service.UpdateLogAsync(dto);
            if (result is null)
            {
                return NotFound(new { message = "Log not found" });
            }

            return Ok(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteLog([FromBody] DeleteLogDto dto)
        {
            var result = await _service.DeleteLogAsync(dto.Id);
            if (!result)
            {
                return NotFound(new { message = "Log not found" });
            }

            return NoContent();
        }

        [HttpPost("GetLogs")]
        public async Task<IActionResult> FetchLogs([FromBody] FetchLogDto dto)
        {
            var result = await _service.FetchLogAsync(dto);
            if (result.Equals(""))
            {
                return NotFound(new { message = "Without log lines" });
            }
            return Ok(result);
        }
    }
}
