using CatalogService.Context;
using CatalogService.DTOs;
using CatalogService.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Services.CinemaService
{
    public class CinemaService : ICinemaService
    {
        private readonly CatalogDbContext _context;
        public CinemaService(CatalogDbContext catalogDbContext)
        {
            _context = catalogDbContext;
        }

        public async Task<List<CinemaDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Cinemas.AsNoTracking().Select(c => new CinemaDto
            {
                Id = c.Id,
                Name = c.Name,
                PhoneNumber = c.PhoneNumber,
                Address = c.Address
            }).ToListAsync(cancellationToken);
        }

        public async Task<CinemaDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty) return null!;

            var result = await _context.Cinemas.AsNoTracking().Select(c => new CinemaDto
            {
                Id = c.Id,
                Name = c.Name,
                PhoneNumber = c.PhoneNumber,
                Address = c.Address
            }).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
            return result!;

        }

        public async Task<bool> UpdateAsync(Guid id, CinemaDto dto, CancellationToken cancellationToken = default)
        {
            var cin = await _context.Cinemas.FirstOrDefaultAsync(c => c.Id == id);
            if (cin == null) return false;

            cin.Name = dto.Name;
            cin.PhoneNumber = dto.PhoneNumber;
            cin.Address = dto.Address;

            await _context.SaveChangesAsync(cancellationToken);
            return true;

        }

        public async Task<CinemaDto> CreateAsync(CinemaDto dto, CancellationToken cancellationToken = default)
        {
            var cinemaName = dto.Name.Trim();
            var exists = await _context.Cinemas.AnyAsync(c => c.Name.ToLower() == cinemaName.ToLower(), cancellationToken);

            if (exists)
            {
                throw new InvalidOperationException("A cinema with this name already exists.");
            }

            var cinema = new Cinema
            {
                Id = Guid.NewGuid(),
                Name = cinemaName,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address
            };

            await _context.AddAsync(cinema, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return new CinemaDto
            {
                Id = dto.Id,
                Name = dto.Name,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address
            };
        }

        public async Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken = default)
        {

                var cinema = await _context.Cinemas.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
                if (cinema == null) return false;

                _context.Cinemas.Remove(cinema);
                await _context.SaveChangesAsync(cancellationToken);
                return true;
        }

    }
}