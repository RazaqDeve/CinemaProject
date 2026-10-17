using CatalogService.DTOs;
using CatalogService.Services.MovieService;
using Microsoft.AspNetCore.Mvc;


namespace CatalogService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovieController : ControllerBase
    {
        private readonly ILogger<MovieController> _logger;
        private readonly MovieService _movieService;

        public MovieController(ILogger<MovieController> logger, MovieService movieService)
        {
            _logger = logger;
            _movieService = movieService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync(CancellationToken cancellation)
        {
            try
            {
                var result = await _movieService.GetAllAsync(cancellation);
                return Ok(result);
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken token)
        {
            try
            {
                if (id == Guid.Empty) return NotFound();
                var result = await _movieService.GetByIdAsync(id, token);
                if (result == null) return NotFound();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erorr in GetBy Id Controller");
                return BadRequest();
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddAsync(MovieDto dto, CancellationToken token)
        {
            try
            {
                var result = await _movieService.Create(dto, token);
                return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Erorr in AddAsync Controller{Erorr}", ex.Message);
                return StatusCode(500);
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> EditAsync(Guid id, MovieDto dto, CancellationToken token)
        {
            try
            {
                if (id == Guid.Empty) return BadRequest();

                var result = await _movieService.Update(id, dto, token);
                    if (!result) return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError("Erorr in EditAsync Controller{Erorr}", ex.Message);
                return BadRequest();
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken token)
        {
            try
            {
                if (id == Guid.Empty) return BadRequest();

                var result = await _movieService.Remove(id, token);
                if (!result) return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError("Erorr in DeleteAsync Controller{Erorr}", ex.Message);
                return BadRequest();
            }
        }
    }
}