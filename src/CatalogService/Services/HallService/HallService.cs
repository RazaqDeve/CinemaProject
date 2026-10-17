using CatalogService.Context;
using CatalogService.DTOs;
using CatalogService.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Services.HallService
{
    public class HallService : IHallService
    {
        private readonly CatalogDbContext _context;

        public HallService(CatalogDbContext catalogDbContext) => _context = catalogDbContext;

        public async Task<List<HallDto>> GetAllAsync(CancellationToken token = default)
        {
            return await _context.Halls
            .AsNoTracking()
            .OrderBy(h => h.Name)
            .Select(h => new HallDto
            {
                Id = h.Id,
                CinemaId = h.CinemaId,
                Name = h.Name
            }).ToListAsync();
        }

        public async Task<List<HallDto>> GetByCinemaIdAsync(Guid cinemaId, CancellationToken token = default)
        {
            return await _context.Halls
            .AsNoTracking()
            .Where(h => h.CinemaId == cinemaId)
            .Select(h => new HallDto
            {
                Id = h.Id,
                CinemaId = h.CinemaId,
                Name = h.Name
            }).ToListAsync(token);
        }

        public async Task<HallDto?> GetByIdAsync(Guid id, CancellationToken token = default)
        {
            return await _context.Halls
            .Where(h => h.Id == id)
            .Select(h => new HallDto
            {
                Id = h.Id,
                CinemaId = h.CinemaId,
                Name = h.Name
            }).FirstOrDefaultAsync(token);
        }

        public async Task<HallDto> CreatAsync(HallDto dto, CancellationToken token = default)
        {
            var hall = new Hall
            {
                Id = Guid.NewGuid(),
                CinemaId = dto.CinemaId,
                Name = dto.Name
            };

            await _context.Halls.AddAsync(hall, token);
            await _context.SaveChangesAsync(token);
            return new HallDto
            {
                Id = hall.Id,
                CinemaId = hall.CinemaId,
                Name = hall.Name
            };
        }

        public async Task<bool> UpdateAsync(Guid id, HallDto dto, CancellationToken token = default)
        {
            var hall = await _context.Halls.FirstOrDefaultAsync(h => h.Id == id,token);
            if (hall == null) return false;

            hall.Name = dto.Name;

            await _context.SaveChangesAsync(token);
            return true;
        }

        public async Task<bool> RemoveAsync(Guid id, CancellationToken token = default)
        {
            var hall = await _context.Halls.FirstOrDefaultAsync(h => h.Id == id,token);
            if (hall == null) return false;

            _context.Halls.Remove(hall);
            await _context.SaveChangesAsync(token);
            return true;
        }
    }
}



