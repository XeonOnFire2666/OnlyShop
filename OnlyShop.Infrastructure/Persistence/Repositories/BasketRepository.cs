using Microsoft.EntityFrameworkCore;
using OnlyShop.Application.Abstractions.Persistence;
using OnlyShop.Core.Entities;

namespace OnlyShop.Infrastructure.Persistence.Repositories;

public class BasketRepository(OnlyShopDbContext db) : IBasketRepository
{
    public Task<Basket?> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        db.Baskets.Include(x => x.Items).FirstOrDefaultAsync(x => x.UserId == userId, ct);

    public void Add(Basket basket) => db.Baskets.Add(basket);
}