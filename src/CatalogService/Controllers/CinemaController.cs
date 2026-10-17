using CatalogService.DTOs;
using CatalogService.Services.CinemaService;
using Microsoft.AspNetCore.Mvc;


namespace CatalogService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CinemaController : ControllerBase
    {
        private readonly ILogger<SeatController> _logger;
        private readonly ICinemaService _cinemaService;

        public CinemaController(ILogger<SeatController> logger, ICinemaService cinemaService)
        {
            _logger = logger;
            _cinemaService = cinemaService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync(CancellationToken token)
        {
            try
            {
                var result = await _cinemaService.GetAllAsync(token);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all cinema controller");
                return StatusCode(500);
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken token)
        {
            if (id == Guid.Empty) return BadRequest();

            try
            {
                var result = await _cinemaService.GetByIdAsync(id, token);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cinema {CinemaId}", id);
                return StatusCode(500);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddAsync(CinemaDto dto, CancellationToken token)
        {
            try
            {
                var result = await _cinemaService.CreateAsync(dto, token);
                return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddAsync cinema Controller");
                return StatusCode(500);
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> EditAsync(Guid id, CinemaDto dto, CancellationToken token)
        {
            if (id == Guid.Empty) return BadRequest();

            try
            {
                var result = await _cinemaService.UpdateAsync(id, dto, token);
                  if(!result) return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ciname {CinemaId}", id);
                return StatusCode(500);
            }
        }

           [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken token)
        {
            if (id == Guid.Empty) return BadRequest();

            try
            {
                var result = await _cinemaService.RemoveAsync(id, token);
                if (!result) return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting cinema {HallId}", id);
                return StatusCode(500);
            }
        }
    }
}