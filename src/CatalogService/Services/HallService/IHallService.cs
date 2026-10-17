using CatalogService.DTOs;



namespace CatalogService.Services.HallService
{
    public interface IHallService
    {
       Task<List<HallDto>> GetAllAsync(CancellationToken token = default);
       Task<HallDto?> GetByIdAsync(Guid id, CancellationToken token = default);
       Task<List<HallDto>> GetByCinemaIdAsync(Guid cinemaId, CancellationToken token = default);
       Task<HallDto> CreatAsync(HallDto hall, CancellationToken token = default);
       Task<bool> UpdateAsync(Guid id, HallDto hall, CancellationToken token = default);
       Task<bool> RemoveAsync(Guid id, CancellationToken token = default);
    }
}