using CatalogService.DTOs;
using CatalogService.Services.SeatService;
using Microsoft.AspNetCore.Mvc;


namespace CatalogService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeatController : ControllerBase
    {
        private readonly ILogger<SeatController> _logger;
        private readonly ISeatService _seatService;

        public SeatController(ILogger<SeatController> logger, ISeatService seatService)
        {
            _logger = logger;
            _seatService = seatService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync(CancellationToken token)
        {
            try
            {
                var result = await _seatService.GetAllAsync(token);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all Seat controller");
                return StatusCode(500);
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken token)
        {
            if (id == Guid.Empty) return BadRequest();

            try
            {
                var result = await _seatService.GetByGuidAsync(id, token);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Seat {SeatId}", id);
                return StatusCode(500);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddAsync(SeatDto dto, CancellationToken token)
        {
            try
            {
                var result = await _seatService.CreatAsync(dto, token);
                return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddAsync Seat Controller");
                return StatusCode(500);
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> EditAsync(Guid id, SeatDto dto, CancellationToken token)
        {
            if (id == Guid.Empty) return BadRequest();

            try
            {
                var result = await _seatService.UpdateAsync(id, dto, token);
                  if(!result) return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating row {RowId}", id);
                return StatusCode(500);
            }
        }

           [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken token)
        {
            if (id == Guid.Empty) return BadRequest();

            try
            {
                var result = await _seatService.RemoveAsync(id, token);
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