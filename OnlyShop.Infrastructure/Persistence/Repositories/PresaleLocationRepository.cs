using Microsoft.EntityFrameworkCore;
using OnlyShop.Application.Abstractions.Persistence;
using OnlyShop.Core.Entities;

namespace OnlyShop.Infrastructure.Persistence.Repositories;

public class PresaleLocationRepository(OnlyShopDbContext db) : IPresaleLocationRepository
{
    public async Task<IReadOnlyList<PresaleLocation>> GetAllAsync(CancellationToken ct = default) =>
        await db.PresaleLocations.ToListAsync(ct);

    public Task<PresaleLocation?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.PresaleLocations.FirstOrDefaultAsync(x => x.Id == id, ct);

    public void Add(PresaleLocation location) => db.PresaleLocations.Add(location);
    public void Remove(PresaleLocation location) => db.PresaleLocations.Remove(location);
}