using OnlyShop.Core.Entities;

namespace OnlyShop.Application.Abstractions.Persistence;

public interface IBasketRepository
{
    Task<Basket?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    void Add(Basket basket);
}