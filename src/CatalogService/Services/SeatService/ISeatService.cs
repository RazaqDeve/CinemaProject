using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CatalogService.DTOs;

namespace CatalogService.Services.SeatService
{
    public interface ISeatService
    {
        Task<IReadOnlyList<SeatDto>> GetAllAsync(CancellationToken token = default);
        Task<SeatDto?> GetByGuidAsync(Guid id, CancellationToken token = default);
        Task<SeatDto> CreatAsync(SeatDto dto, CancellationToken token = default);
        Task<bool> UpdateAsync(Guid id, SeatDto dto, CancellationToken token = default);
        Task<bool> RemoveAsync(Guid id, CancellationToken token = default);
    }
}