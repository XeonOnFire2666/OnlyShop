using OnlyShop.Core.Common;
using OnlyShop.Core.Enums;

namespace OnlyShop.Core.Entities;

public class OrderStatusChange : BaseEntity
{
    private OrderStatusChange()
    {
    }

    public Guid OrderId { get; private set; }
    public OrderStatus From { get; private set; }
    public OrderStatus To { get; private set; }
    public Guid? ChangedById { get; private set; }
    public string? Note { get; private set; }

    internal static OrderStatusChange Create(
        Guid orderId,
        OrderStatus from,
        OrderStatus to,
        Guid? changedById,
        string? note) => new()
        {
            OrderId = orderId,
            From = from,
            To = to,
            ChangedById = changedById,
            Note = note
        };
}