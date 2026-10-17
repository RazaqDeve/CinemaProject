
using CatalogService.DTOs;
using CatalogService.Entities;

namespace CatalogService.Services.CinemaService
{
    public interface ICinemaService
    {
        Task<List<CinemaDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<CinemaDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<CinemaDto> CreateAsync(CinemaDto cinema, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(Guid id, CinemaDto cinema, CancellationToken cancellationToken = default);
        Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken = default);
    }
}