using CatalogService.DTOs;


namespace CatalogService.Services.MovieService
{
    public interface IMovieService
    {
        Task<List<MovieDto>> GetAllAsync(CancellationToken token = default);
        Task<MovieDto?> GetByIdAsync(Guid id,CancellationToken token = default);
        Task<MovieDto> Create(MovieDto dto, CancellationToken token = default);
        Task<bool> Update(Guid id, MovieDto dto, CancellationToken token = default);
        Task<bool> Remove(Guid id, CancellationToken token = default);
    }
}