using OnlyShop.Application.Dtos;

namespace OnlyShop.Application.Abstractions.Services;

public interface IBasketService
{
    Task<BasketDto> GetAsync(Guid userId, CancellationToken ct = default);
    Task<BasketDto> AddItemAsync(Guid userId, AddToBasketRequest request, CancellationToken ct = default);
    Task<BasketDto> SetQuantityAsync(Guid userId, Guid variantId, int quantity, CancellationToken ct = default);
    Task<BasketDto> RemoveItemAsync(Guid userId, Guid variantId, CancellationToken ct = default);
    Task<BasketDto> ClearAsync(Guid userId, CancellationToken ct = default);
}