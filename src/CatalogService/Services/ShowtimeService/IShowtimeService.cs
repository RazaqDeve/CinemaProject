using CatalogService.DTOs;

namespace CatalogService.Services.ShowtimeService
{
    public interface IShowtimeService
    {
        Task<List<ShowtimeDto>> GetAllAsync(CancellationToken token = default);
        Task<ShowtimeDto?> GetByGuidAsync(Guid id, CancellationToken token = default);
        Task<List<ShowtimeDto>> GetByHallIdAsync(Guid hallId, CancellationToken token = default);
        Task<List<ShowtimeDto>> GetByMovieIdAsync(Guid movieId, CancellationToken token = default);
        Task<ShowtimeDto> CreateAsync(ShowtimeDto dto, CancellationToken token = default);
        Task<bool> UpdateAsync(Guid id, ShowtimeDto dto, CancellationToken token = default);
        Task<bool> RemoveAsync(Guid id, CancellationToken token = default);

    }
}