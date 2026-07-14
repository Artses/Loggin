using Api_Loggin.DTOs;
using Api_Loggin.Models;
using Api_Loggin.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api_Loggin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User")]
    public class CollectorController : ControllerBase
    {
        private readonly ICollectorService _service;

        public CollectorController(ICollectorService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> AddCollector([FromBody] RegisterCollectorDto dto)
        {

            var result = await _service.RegisterCollectorAsync(dto);
            if (result is null)
            {
                return BadRequest(new { message = "Failed to register collector" });
            }

            return CreatedAtAction(nameof(GetCollectorById), new { id = result.Id }, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCollectors()
        {
            var collectors = await _service.GetAllCollectorsAsync();
            return Ok(collectors);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetCollectorById(Guid id)
        {
            var collector = await _service.GetCollectorAsync(id);
            if (collector is null)
            {
                return NotFound(new { message = "Collector not found" });
            }

            return Ok(collector);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCollector([FromBody] UpdateCollectorDto dto)
        {
         

            var result = await _service.UpdateCollectorAsync(dto);
            if (result is null)
            {
                return NotFound(new { message = "Collector not found" });
            }

            return Ok(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteCollector([FromBody] DeleteCollectorDto dto)
        {
            var result = await _service.DeleteCollectorAsync(dto.id);
            if (!result)
            {
                return NotFound(new { message = "Collector not found" });
            }

            return NoContent();
        }
    }
}
