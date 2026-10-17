using CatalogService.DTOs;
using CatalogService.Services.RowSerivce;
using Microsoft.AspNetCore.Mvc;


namespace CatalogService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RowController : ControllerBase
    {
        private readonly ILogger<RowController> _logger;
        private readonly IRowService _rowService;

        public RowController(ILogger<RowController> logger, IRowService rowService)
        {
            _logger = logger;
            _rowService = rowService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync(CancellationToken token)
        {
            try
            {
                var result = await _rowService.GetAllAsync(token);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all row controller");
                return StatusCode(500);
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken token)
        {
            if (id == Guid.Empty) return BadRequest();
            try
            {
                var result = await _rowService.GetByIdAsync(id, token);
                if (result == null) return NotFound();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting row {RowId}", id);
                return StatusCode(500);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddAsync(RowDto dto, CancellationToken token)
        {
            try
            {
                var result = await _rowService.Create(dto, token);
                return CreatedAtAction(nameof(GetByIdAsync), new {id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddAsync row Controller");
                return StatusCode(500);
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> EditAsync(Guid id, RowDto dto, CancellationToken token)
        {
            if (id == Guid.Empty) return BadRequest();

            try
            {
                var result = await _rowService.Update(id, dto, token);
                if (!result) return NotFound();

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
                var result = await _rowService.Remove(id, token);
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