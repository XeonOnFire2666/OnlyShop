using OnlyShop.Application.Dtos;

namespace OnlyShop.Application.Abstractions.Services;

public interface IStaffService
{
    Task<StaffDto> AuthenticateAsync(string email, string password, CancellationToken ct = default);
    Task<IReadOnlyList<StaffDto>> GetAllAsync(CancellationToken ct = default);
    Task<StaffDto> CreateAsync(CreateStaffRequest request, CancellationToken ct = default);
    Task SetActiveAsync(Guid staffId, bool isActive, CancellationToken ct = default);
    Task LinkTelegramAsync(Guid staffId, long telegramId, CancellationToken ct = default);
}