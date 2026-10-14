using OnlyShop.Core.Entities;

namespace OnlyShop.Application.Abstractions.Persistence;

public interface IStaffRepository
{
    Task<StaffMember?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<StaffMember?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<IReadOnlyList<StaffMember>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<long>> GetNotificationTelegramIdsAsync(CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    void Add(StaffMember staff);
}