using OnlyShop.Application.Abstractions.Persistence;
using OnlyShop.Application.Abstractions.Services;
using OnlyShop.Application.Common.Exceptions;
using OnlyShop.Application.Dtos;
using OnlyShop.Application.Mapping;
using OnlyShop.Core.Common;
using OnlyShop.Core.Entities;

namespace OnlyShop.Application.Services;

internal sealed class BasketService(
    IBasketRepository baskets,
    IProductRepository products,
    ProductMapper mapper,
    IUnitOfWork uow) : IBasketService
{
    public async Task<BasketDto> GetAsync(Guid userId, CancellationToken ct = default)
    {
        var basket = await baskets.GetByUserIdAsync(userId, ct);
        return basket is null ? BasketDto.Empty : await BuildAsync(basket, ct);
    }

    public async Task<BasketDto> AddItemAsync(Guid userId, AddToBasketRequest request, CancellationToken ct = default)
    {
        var product = await products.GetByIdAsync(request.ProductId, ct)
            ?? throw new NotFoundException(nameof(Product), request.ProductId);

        var variant = FindVariant(product, request.VariantId);
        var basket = await GetOrCreateAsync(userId, ct);

        var existingItem = basket.Items
            .FirstOrDefault(i => i.ProductVariantId == variant.Id);

        var existingQuantity = existingItem?.Quantity ?? 0;

        EnsureCanOrder(product, variant, existingQuantity + request.Quantity);

        basket.AddItem(product.Id, variant.Id, request.Quantity);

        if (existingItem == null)
        {
            var newItem = basket.Items.Last();      
            uow.Add(newItem);       
        }

        await uow.SaveChangesAsync(ct);

        return await BuildAsync(basket, ct);
    }

    public async Task<BasketDto> SetQuantityAsync(Guid userId, Guid variantId, int quantity, CancellationToken ct = default)
    {
        var basket = await baskets.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException(nameof(Basket), userId);

        var item = basket.Items.FirstOrDefault(i => i.ProductVariantId == variantId)
            ?? throw new NotFoundException(nameof(BasketItem), variantId);

        var product = await products.GetByIdAsync(item.ProductId, ct)
            ?? throw new NotFoundException(nameof(Product), item.ProductId);

        EnsureCanOrder(product, FindVariant(product, variantId), quantity);

        basket.SetQuantity(variantId, quantity);
        await uow.SaveChangesAsync(ct);

        return await BuildAsync(basket, ct);
    }

    public async Task<BasketDto> RemoveItemAsync(Guid userId, Guid variantId, CancellationToken ct = default)
    {
        var basket = await baskets.GetByUserIdAsync(userId, ct);

        if (basket is null)
        {
            return BasketDto.Empty;
        }

        basket.RemoveItem(variantId);
        await uow.SaveChangesAsync(ct);

        return await BuildAsync(basket, ct);
    }

    public async Task<BasketDto> ClearAsync(Guid userId, CancellationToken ct = default)
    {
        var basket = await baskets.GetByUserIdAsync(userId, ct);

        if (basket is null)
        {
            return BasketDto.Empty;
        }

        basket.Clear();
        await uow.SaveChangesAsync(ct);

        return BasketDto.Empty;
    }

    private async Task<Basket> GetOrCreateAsync(Guid userId, CancellationToken ct)
    {
        var basket = await baskets.GetByUserIdAsync(userId, ct);

        if (basket is not null)
        {
            return basket;
        }

        basket = Basket.Create(userId);
        baskets.Add(basket);
        return basket;
    }

    private static ProductVariant FindVariant(Product product, Guid variantId) =>
        product.Variants.FirstOrDefault(v => v.Id == variantId)
            ?? throw new NotFoundException(nameof(ProductVariant), variantId);

    private static void EnsureCanOrder(Product product, ProductVariant variant, int quantity)
    {
        if (!product.CanBeOrdered)
        {
            throw new DomainException("Product is not available for ordering");
        }

        if (!product.IsPresale && quantity > variant.StockQuantity)
        {
            throw new DomainException($"Only {variant.StockQuantity} left in size {variant.Size}");
        }
    }

    private async Task<BasketDto> BuildAsync(Basket basket, CancellationToken ct)
    {
        var ids = basket.Items.Select(i => i.ProductId).Distinct().ToList();
        var byId = (await products.GetByIdsAsync(ids, ct)).ToDictionary(p => p.Id);
        var items = new List<BasketItemDto>();

        foreach (var item in basket.Items)
        {
            if (!byId.TryGetValue(item.ProductId, out var product))
            {
                continue;
            }

            var variant = product.Variants.FirstOrDefault(v => v.Id == item.ProductVariantId);

            if (variant is null)
            {
                continue;
            }

            var isAvailable = product.CanBeOrdered
                && (product.IsPresale || variant.StockQuantity >= item.Quantity);

            items.Add(new BasketItemDto(
                product.Id,
                variant.Id,
                product.Name,
                variant.Size,
                mapper.CoverUrl(product),
                product.Price.Byn,
                product.Price.Rub,
                item.Quantity,
                isAvailable));
        }

        return new BasketDto(
            items,
            items.Sum(i => i.Quantity),
            items.Sum(i => i.PriceByn * i.Quantity),
            items.Sum(i => i.PriceRub * i.Quantity));
    }
}