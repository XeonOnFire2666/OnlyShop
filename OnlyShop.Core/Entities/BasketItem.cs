using OnlyShop.Core.Common;

namespace OnlyShop.Core.Entities;

public class BasketItem : BaseEntity
{
    private BasketItem()
    {
    }

    public Guid BasketId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid ProductVariantId { get; private set; }
    public int Quantity { get; private set; }

    internal static BasketItem Create(Guid basketId, Guid productId, Guid variantId, int quantity)
    {
        EnsurePositive(quantity);

        return new BasketItem
        {
            BasketId = basketId,
            ProductId = productId,
            ProductVariantId = variantId,
            Quantity = quantity
        };
    }

    internal void Increase(int quantity)
    {
        EnsurePositive(quantity);
        Quantity += quantity;
    }

    internal void SetQuantity(int quantity)
    {
        EnsurePositive(quantity);
        Quantity = quantity;
    }

    private static void EnsurePositive(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("Quantity must be positive");
        }
    }
}