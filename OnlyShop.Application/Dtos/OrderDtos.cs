using OnlyShop.Core.Enums;

namespace OnlyShop.Application.Dtos;

public sealed record CheckoutRequest(
    Currency Currency,
    PaymentMethod PaymentMethod,
    DeliveryMethod DeliveryMethod,
    string? DeliveryAddress,
    string? Comment,
    string? Phone,
    decimal BonusesToSpend = 0m);

public sealed record OrderItemDto(
    Guid ProductId,
    string ProductName,
    string Size,
    decimal UnitPrice,
    int Quantity,
    decimal Total);

public sealed record OrderStatusChangeDto(
    OrderStatus From,
    OrderStatus To,
    Guid? ChangedById,
    string? Note,
    DateTime At);

public sealed record OrderDto(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    string? CustomerPhone,
    OrderStatus Status,
    Currency Currency,
    PaymentMethod PaymentMethod,
    DeliveryMethod DeliveryMethod,
    string? DeliveryAddress,
    string? Comment,
    decimal BonusesSpent,
    decimal ItemsTotal,
    decimal Total,
    DateTime CreatedAt,
    IReadOnlyList<OrderItemDto> Items,
    IReadOnlyList<OrderStatusChangeDto> History,
    IReadOnlyList<OrderStatus> AllowedTransitions);

public sealed record OrderDetailsDto(OrderDto Order, UserDto Customer);

public sealed record AdminOrderQuery(
    OrderStatus? Status = null,
    string? Search = null,
    DateTime? From = null,
    DateTime? To = null,
    int Page = 1,
    int PageSize = 20);

public sealed record ChangeOrderStatusRequest(OrderStatus Status, string? Note);