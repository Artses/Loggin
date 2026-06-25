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
            var collector = new Collector
            {
                Name = dto.Name,
                Url = dto.Url,
                Path = dto.Path
            };

            var result = await _service.RegisterCollectorAsync(collector);
            if (!result)
            {
                return BadRequest(new { message = "Failed to register collector" });
            }

            return CreatedAtAction(nameof(GetCollectorById), new { id = collector.Id }, collector);
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
            var collector = new Collector
            {
                Id = dto.Id,
                Name = dto.Name,
                Url = dto.Url,
                Path = dto.Path
            };

            var result = await _service.UpdateCollectorAsync(collector);
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
