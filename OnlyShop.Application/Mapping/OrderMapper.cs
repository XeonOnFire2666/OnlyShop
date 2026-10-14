using OnlyShop.Application.Dtos;
using OnlyShop.Core.Entities;
using OnlyShop.Core.Enums;

namespace OnlyShop.Application.Mapping;

internal static class OrderMapper
{
    public static OrderDto ToDto(this Order order) => new(
        order.Id,
        order.CustomerId,
        order.CustomerName,
        order.CustomerPhone,
        order.Status,
        order.Currency,
        order.PaymentMethod,
        order.DeliveryMethod,
        order.DeliveryAddress,
        order.Comment,
        order.BonusesSpent,
        order.ItemsTotal,
        order.Total,
        order.CreatedAt,
        order.Items
            .Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.Size, i.UnitPrice, i.Quantity, i.Total))
            .ToList(),
        order.StatusHistory
            .OrderBy(h => h.CreatedAt)
            .Select(h => new OrderStatusChangeDto(h.From, h.To, h.ChangedById, h.Note, h.CreatedAt))
            .ToList(),
        Enum.GetValues<OrderStatus>().Where(order.CanTransitionTo).ToList());
}