using OnlyShop.Core.Common;
using OnlyShop.Core.ValueObjects;

namespace OnlyShop.Core.Entities;

public class OrderItem : BaseEntity
{
    private OrderItem()
    {
    }

    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = null!;
    public string Size { get; private set; } = null!;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }

    public decimal Total => UnitPrice * Quantity;

    internal static OrderItem Create(Guid orderId, NewOrderItem data)
    {
        if (data.Quantity <= 0)
        {
            throw new DomainException("Quantity must be positive");
        }

        if (data.UnitPrice <= 0)
        {
            throw new DomainException("Unit price must be positive");
        }

        return new OrderItem
        {
            OrderId = orderId,
            ProductId = data.ProductId,
            ProductName = data.ProductName,
            Size = data.Size,
            UnitPrice = data.UnitPrice,
            Quantity = data.Quantity
        };
    }
}