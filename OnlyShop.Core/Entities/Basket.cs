using OnlyShop.Core.Common;

namespace OnlyShop.Core.Entities;

public class Basket : BaseEntity
{
    private readonly List<BasketItem> _items = [];

    private Basket()
    {
    }

    public Guid UserId { get; private set; }

    public IReadOnlyList<BasketItem> Items => _items;
    public int TotalQuantity => _items.Sum(i => i.Quantity);

    public static Basket Create(Guid userId) => new() { UserId = userId };

    public void AddItem(Guid productId, Guid variantId, int quantity)
    {
        var existing = _items.FirstOrDefault(i => i.ProductVariantId == variantId);

        if (existing is null)
        {
            _items.Add(BasketItem.Create(Id, productId, variantId, quantity));
        }
        else
        {
            existing.Increase(quantity);
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void SetQuantity(Guid variantId, int quantity)
    {
        var item = _items.FirstOrDefault(i => i.ProductVariantId == variantId)
            ?? throw new DomainException("Item not found in basket");

        item.SetQuantity(quantity);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveItem(Guid variantId)
    {
        _items.RemoveAll(i => i.ProductVariantId == variantId);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Clear()
    {
        _items.Clear();
        UpdatedAt = DateTime.UtcNow;
    }
}