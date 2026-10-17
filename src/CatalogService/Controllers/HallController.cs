using CatalogService.DTOs;
using CatalogService.Services.HallService;
using Microsoft.AspNetCore.Mvc;


namespace CatalogService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HallController : ControllerBase
    {
        private readonly ILogger<HallController> _logger;
        private readonly IHallService _hallService;

        public HallController(ILogger<HallController> logger, IHallService hallService)
        {
            _logger = logger;
            _hallService = hallService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync(CancellationToken token)
        {
            try
            {
                var result = await _hallService.GetAllAsync(token);
                return Ok(result);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error getting all halls controller");
                return StatusCode(500);
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken token)
        {
            if (id == Guid.Empty) return BadRequest();

            try
            {
                var result = await _hallService.GetByIdAsync(id, token);
                if (result == null) return NotFound();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hall {HallId}", id);
                return StatusCode(500);
            }
        }

        [HttpGet("cinema/{cinemaId:guid}")]
        public async Task<IActionResult> GetByCinemaIdAsync(Guid cinemaId, CancellationToken token)
        {
            if (cinemaId == Guid.Empty) return BadRequest();

            try
            {
                var result = await _hallService.GetByCinemaIdAsync(cinemaId, token);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting halls for cinema {CinemaId}", cinemaId);
                return StatusCode(500);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddAsync(HallDto dto, CancellationToken token)
        {
            try
            {
                var result = await _hallService.CreatAsync(dto, token);
                return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddAsync Controller");
                return StatusCode(500);
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> EditAsync(Guid id, HallDto dto, CancellationToken token)
        {
            if (id == Guid.Empty) return BadRequest();

            try
            {
                var result = await _hallService.UpdateAsync(id, dto, token);
                if (!result) return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating hall {HallId}", id);
                return StatusCode(500);
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken token)
        {
            if (id == Guid.Empty) return BadRequest();

            try
            {
                var result = await _hallService.RemoveAsync(id, token);
                if (!result) return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting hall {HallId}", id);
                return StatusCode(500);
            }
        }
    }
}