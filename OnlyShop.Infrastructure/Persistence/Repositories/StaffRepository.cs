using Microsoft.EntityFrameworkCore;
using OnlyShop.Application.Abstractions.Persistence;
using OnlyShop.Core.Entities;

namespace OnlyShop.Infrastructure.Persistence.Repositories;

public class StaffRepository(OnlyShopDbContext db) : IStaffRepository
{
    public Task<StaffMember?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.StaffMembers.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<StaffMember?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        db.StaffMembers.FirstOrDefaultAsync(x => x.Email == email, ct);

    public Task<IReadOnlyList<StaffMember>> GetAllAsync(CancellationToken ct = default) =>
        db.StaffMembers.OrderBy(x => x.Name).ToListAsync(ct)
            .ContinueWith(t => (IReadOnlyList<StaffMember>)t.Result, ct);

    public async Task<IReadOnlyList<long>> GetNotificationTelegramIdsAsync(CancellationToken ct = default) =>
        await db.StaffMembers
            .Where(x => x.IsActive && x.TelegramId != null)
            .Select(x => x.TelegramId!.Value)
            .ToListAsync(ct);

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default) =>
        db.StaffMembers.AnyAsync(x => x.Email == email, ct);

    public void Add(StaffMember staff) => db.StaffMembers.Add(staff);
}