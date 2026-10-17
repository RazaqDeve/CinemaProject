using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CatalogService.Context;
using CatalogService.DTOs;
using CatalogService.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Services.ShowtimeService
{
    public class ShowtimeService : IShowtimeService
    {
        private readonly CatalogDbContext _context;
        public ShowtimeService(CatalogDbContext catalogDbContext) => _context = catalogDbContext;

        public Task<List<ShowtimeDto>> GetAllAsync(CancellationToken token = default)
        {
            return _context.Showtimes.AsNoTracking().Select(s => new ShowtimeDto
            {
                Id = s.Id,
                HallId = s.HallId,
                MovieId = s.MovieId,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                Price = s.Price
            }).ToListAsync(token);
        }

        public async Task<ShowtimeDto?> GetByGuidAsync(Guid id, CancellationToken token = default)
        {
            return await _context.Showtimes.AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new ShowtimeDto
            {
                Id = s.Id,
                HallId = s.HallId,
                MovieId = s.MovieId,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                Price = s.Price
            }).FirstOrDefaultAsync(token);
        }

        public async Task<List<ShowtimeDto>> GetByHallIdAsync(Guid hallId, CancellationToken token = default)
        {
            return await _context.Showtimes.AsNoTracking()
            .Where(s => s.HallId == hallId)
            .Select(s => new ShowtimeDto
            {
                Id = s.Id,
                HallId = s.HallId,
                MovieId = s.MovieId,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                Price = s.Price
            }).ToListAsync(token);
        }

        public async Task<List<ShowtimeDto>> GetByMovieIdAsync(Guid movieId, CancellationToken token = default)
        {
            return await _context.Showtimes.AsNoTracking()
            .Where(s => s.MovieId == movieId)
           .Select(s => new ShowtimeDto
           {
               Id = s.Id,
               HallId = s.HallId,
               MovieId = s.MovieId,
               StartTime = s.StartTime,
               EndTime = s.EndTime,
               Price = s.Price
           }).ToListAsync(token);
        }

        public async Task<ShowtimeDto> CreateAsync(ShowtimeDto dto, CancellationToken token = default)
        {
            await ValidateShowtimeAsync(dto, null, token);

            var show = new Showtime
            {
                Id = Guid.NewGuid(),
                HallId = dto.HallId,
                MovieId = dto.MovieId,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Price = dto.Price
            };

            _context.Showtimes.Add(show);
            await _context.SaveChangesAsync(token);
            return new ShowtimeDto
            {
                Id = show.Id,
                HallId = show.HallId,
                MovieId = show.MovieId,
                StartTime = show.StartTime,
                EndTime = show.EndTime,
                Price = show.Price
            };
        }

        public async Task<bool> UpdateAsync(Guid id, ShowtimeDto dto, CancellationToken token = default)
        {
            var result = await _context.Showtimes.FirstOrDefaultAsync(s => s.Id == id, token);
            if (result == null) return false;

            await ValidateShowtimeAsync(dto, id, token);

            result.MovieId = dto.MovieId;
            result.HallId = dto.HallId;
            result.StartTime = dto.StartTime;
            result.EndTime = dto.EndTime;
            result.Price = dto.Price;

            await _context.SaveChangesAsync(token);
            return true;
        }

        public async Task<bool> RemoveAsync(Guid id, CancellationToken token = default)
        {
            var result = await _context.Showtimes.FirstOrDefaultAsync(s => s.Id == id, token);
            if (result == null) return false;

            _context.Showtimes.Remove(result);
            await _context.SaveChangesAsync(token);
            return true;
        }

        private async Task ValidateShowtimeAsync(ShowtimeDto dto, Guid? excludeShowtimeId, CancellationToken token)
        {
            if (dto.MovieId == Guid.Empty) throw new ValidationException("MovieId is required.");

            if (dto.HallId == Guid.Empty) throw new ValidationException("HallId is required.");

            if (dto.StartTime == default || dto.EndTime == default) throw new ValidationException("StartTime and EndTime are required.");

            if (dto.EndTime <= dto.StartTime) throw new ValidationException("EndTime must be later than StartTime.");

            // 3. بررسی قیمت
            if (dto.Price < 0) throw new ValidationException("Price cannot be negative.");

            // 4. بررسی وجود فیلم
            var movieExists = await _context.Movies.AnyAsync(m => m.Id == dto.MovieId, token);
            if (!movieExists) throw new ValidationException("The specified movie does not exist.");

            // 5. بررسی وجود سالن
            var hallExists = await _context.Halls.AnyAsync(h => h.Id == dto.HallId, token);
            if (!hallExists) throw new ValidationException("The specified hall does not exist.");

            // 6. بررسی تداخل زمانی در همان سالن
            var hasOverlap = await _context.Showtimes.AnyAsync(s => s.HallId == dto.HallId
            && (!excludeShowtimeId.HasValue || s.Id != excludeShowtimeId.Value)
            && s.StartTime < dto.EndTime
            && s.EndTime > dto.StartTime, token);

            if (hasOverlap) throw new ValidationException("Another showtime already exists in this hall during the selected time.");
        }
    }
}