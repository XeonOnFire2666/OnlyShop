using OnlyShop.Application.Common.Models;
using OnlyShop.Application.Dtos;

namespace OnlyShop.Application.Abstractions.Services;

public interface IPresaleLocationService
{
    Task<IReadOnlyList<PresaleLocationDto>> GetAllAsync(CancellationToken ct = default);
    Task<PresaleLocationDto> CreateAsync(PresaleLocationRequest request, FileUpload? photo, CancellationToken ct = default);
    Task<PresaleLocationDto> UpdateAsync(Guid locationId, PresaleLocationRequest request, FileUpload? photo, CancellationToken ct = default);
    Task DeleteAsync(Guid locationId, CancellationToken ct = default);
}