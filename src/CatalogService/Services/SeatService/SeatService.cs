using CatalogService.Context;
using CatalogService.DTOs;
using CatalogService.Entities;
using Microsoft.EntityFrameworkCore;



namespace CatalogService.Services.SeatService
{
    public class SeatService : ISeatService
    {
        private readonly CatalogDbContext _context;

        public SeatService(CatalogDbContext catalogDbContext) => _context = catalogDbContext;

        public async Task<IReadOnlyList<SeatDto>> GetAllAsync(CancellationToken token = default)
        {
           return await _context.Seats.AsNoTracking().Select(s => new SeatDto
           {
               Id = s.Id,
               RowId = s.RowId,
               SeatNumber = s.SeatNumber
           }).ToListAsync(token);
        }

        public async Task<SeatDto?> GetByGuidAsync(Guid id, CancellationToken token = default)
        {
            return await _context.Seats
            .AsNoTracking()
            .Where(s => s.Id == id).Select(s => new SeatDto
            {
                Id = s.Id,
                RowId = s.RowId,
                SeatNumber = s.SeatNumber
            }).FirstOrDefaultAsync(token);
        }

        public async Task<SeatDto> CreatAsync(SeatDto dto, CancellationToken token = default)
        {
            var seat = new Seat
            {
                Id = Guid.NewGuid(),
                RowId = dto.RowId,
                SeatNumber = dto.SeatNumber 
            };

            _context.Seats.Add(seat);
            await _context.SaveChangesAsync(token);
            return new SeatDto
            {
                Id = seat.Id,
                RowId = seat.RowId,
                SeatNumber = seat.SeatNumber
            };
        }

        public async Task<bool> UpdateAsync(Guid id, SeatDto dto, CancellationToken token = default)
        {
           var result = await _context.Seats.FirstOrDefaultAsync(s => s.Id == id);
           if(result == null) return false;

           dto.SeatNumber = result.SeatNumber;

           await _context.SaveChangesAsync(token);
           return true;
        }

        public async Task<bool> RemoveAsync(Guid id, CancellationToken token = default)
        {
            var result = await _context.Seats.FirstOrDefaultAsync(s => s.Id == id);
            if(result == null) return false;

            _context.Seats.Remove(result);
            await _context.SaveChangesAsync(token);
            return true;
        }
    }
}