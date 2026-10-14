using Microsoft.EntityFrameworkCore;
using OnlyShop.Application.Abstractions.Persistence;
using OnlyShop.Core.Entities;

namespace OnlyShop.Infrastructure.Persistence.Repositories;

public class UserRepository(OnlyShopDbContext db) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Users.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<User?> GetByTelegramIdAsync(long telegramId, CancellationToken ct = default) =>
        db.Users.FirstOrDefaultAsync(x => x.TelegramId == telegramId, ct);

    public void Add(User user) => db.Users.Add(user);
}