using CatalogService.DTOs;

namespace CatalogService.Services.RowSerivce
{
    public interface IRowService
    {
        Task<List<RowDto>> GetAllAsync(CancellationToken token = default);
        Task<RowDto?> GetByIdAsync(Guid id, CancellationToken token = default);
        Task<RowDto> Create(RowDto dto, CancellationToken token = default);
        Task<bool> Update(Guid id, RowDto dto, CancellationToken token = default);
        Task<bool> Remove(Guid id, CancellationToken token = default);
    }
}
