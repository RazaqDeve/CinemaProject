using System.ComponentModel.DataAnnotations;
using CatalogService.DTOs;
using CatalogService.Services.ShowtimeService;
using Microsoft.AspNetCore.Mvc;


namespace CatalogService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShowtimeController : ControllerBase
    {
        private readonly ILogger<ShowtimeController> _logger;
        private readonly IShowtimeService _showtime;

        public ShowtimeController(ILogger<ShowtimeController> logger, IShowtimeService showtimeService)
        {
            _logger = logger;
            _showtime = showtimeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync(CancellationToken token)
        {
            try
            {
                var result = await _showtime.GetAllAsync(token);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all show controller");
                return StatusCode(500);
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken token)
        {
            if (id == Guid.Empty) return BadRequest();

            try
            {
                var result = await _showtime.GetByGuidAsync(id, token);
                if (result == null) return NotFound();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting show {ShowId}", id);
                return StatusCode(500);
            }
        }

        [HttpGet("hall/{halId:guid}")]
        public async Task<IActionResult> GetByHallIdAsync(Guid halId, CancellationToken token)
        {
            if (halId == Guid.Empty) return BadRequest();
            try
            {
                var result = await _showtime.GetByHallIdAsync(halId, token);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting show {hallId}", halId);
                return StatusCode(500);
            }
        }

        [HttpGet("movie/{movieId:guid}")]
        public async Task<IActionResult> GetByMovieIdAsync(Guid movieId, CancellationToken token)
        {
            if (movieId == Guid.Empty) return BadRequest();
            try
            {
                var result = await _showtime.GetByMovieIdAsync(movieId, token);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting show {movieId}", movieId);
                return StatusCode(500);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddAsync(ShowtimeDto dto, CancellationToken token)
        {
            try
            {
                var result = await _showtime.CreateAsync(dto, token);
                return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Id }, result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddAsync Show Controller");
                return StatusCode(500);
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> EditAsync(Guid id, ShowtimeDto dto, CancellationToken token)
        {
            if (id == Guid.Empty) return BadRequest();

            try
            {
                var result = await _showtime.UpdateAsync(id, dto, token);
                if (!result) return NotFound();

                return NoContent();
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating show {ShowId}", id);
                return StatusCode(500);
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken token)
        {
            if (id == Guid.Empty) return BadRequest();

            try
            {
                var result = await _showtime.RemoveAsync(id, token);
                if (!result) return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Show {ShowId}", id);
                return StatusCode(500);
            }
        }
    }
}