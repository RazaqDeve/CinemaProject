using CatalogService.Context;
using CatalogService.DTOs;
using CatalogService.Entities;
using Microsoft.EntityFrameworkCore;


namespace CatalogService.Services.RowSerivce
{
    public class RowService : IRowService
    {
          private readonly CatalogDbContext _context;
        public RowService(CatalogDbContext catalogDbContext) => _context = catalogDbContext;


        public async Task<List<RowDto>> GetAllAsync(CancellationToken token = default)
        {

            return await _context.Rows.AsNoTracking().Select(r => new RowDto
            {
                Id = r.Id,
                HallId = r.HallId,
                RowNumber = r.RowNumber
            }).ToListAsync(token);
        }

        public async Task<RowDto?> GetByIdAsync(Guid id, CancellationToken token = default)
        {
            return await _context.Rows.
            AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new RowDto
            {
                Id = r.Id,
                HallId = r.HallId,
                RowNumber = r.RowNumber
            }).FirstOrDefaultAsync(token);
        }

        public async Task<RowDto> Create(RowDto dto, CancellationToken token = default)
        {
            var row = new Row
            {
                Id = Guid.NewGuid(),
                HallId = dto.HallId,
                RowNumber = dto.RowNumber
            };

            await _context.Rows.AddAsync(row, token);
            await _context.SaveChangesAsync(token);
            return new RowDto
            {
                Id = row.Id,
                HallId = row.HallId,
                RowNumber = dto.RowNumber
            };
        }

        public async Task<bool> Update(Guid id, RowDto dto, CancellationToken token = default)
        {
            var row = await _context.Rows.FirstOrDefaultAsync(m => m.Id == id, token);
            if (row == null) return false;

            row.RowNumber = dto.RowNumber;

            await _context.SaveChangesAsync(token);
            return true;
        }

        public async Task<bool> Remove(Guid id, CancellationToken token = default)
        {
            var result = await _context.Rows.FirstOrDefaultAsync(r => r.Id == id, token);
            if (result is null) return false;

            _context.Rows.Remove(result);
            await _context.SaveChangesAsync(token);
            return true;
        }

    }
}