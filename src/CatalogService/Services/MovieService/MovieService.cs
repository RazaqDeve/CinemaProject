using CatalogService.Context;
using CatalogService.DTOs;
using CatalogService.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Services.MovieService
{
    public class MovieService : IMovieService
    {
        private readonly CatalogDbContext _context;
        public MovieService(CatalogDbContext catalogDbContext) => _context = catalogDbContext;


        public async Task<List<MovieDto>> GetAllAsync(CancellationToken token = default)
        {

            return await _context.Movies.AsNoTracking().Select(m => new MovieDto
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                DurationMinutes = m.DurationMinutes,
                ReleaseDate = m.ReleaseDate
            }).ToListAsync(token);
        }

        public async Task<MovieDto?> GetByIdAsync(Guid id, CancellationToken token = default)
        {
            return await _context.Movies.
            AsNoTracking()
            .Where(m => m.Id == id)
            .Select(m => new MovieDto
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                DurationMinutes = m.DurationMinutes,
                ReleaseDate = m.ReleaseDate
            }).FirstOrDefaultAsync(token);
        }

        public async Task<MovieDto> Create(MovieDto dto, CancellationToken token = default)
        {
            var movie = new Movie
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                DurationMinutes = dto.DurationMinutes,
                ReleaseDate = dto.ReleaseDate
            };

            await _context.Movies.AddAsync(movie, token);
            await _context.SaveChangesAsync(token);
            return new MovieDto
            {
                Id = movie.Id,
                Title = movie.Title,
                Description = movie.Description,
                DurationMinutes = movie.DurationMinutes,
                ReleaseDate = movie.ReleaseDate
            };
        }

        public async Task<bool> Update(Guid id, MovieDto dto, CancellationToken token = default)
        {
            var movie = await _context.Movies.FirstOrDefaultAsync(m => m.Id == id, token);
            if (movie == null) return false;

            movie.Title = dto.Title;
            movie.Description = dto.Description;
            movie.DurationMinutes = dto.DurationMinutes;
            movie.ReleaseDate = dto.ReleaseDate;

            await _context.SaveChangesAsync(token);
            return true;
        }

        public async Task<bool> Remove(Guid id, CancellationToken token = default)
        {
            var result = await _context.Movies.FirstOrDefaultAsync(m => m.Id == id);
            if (result is null) return false;

            _context.Movies.Remove(result);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}