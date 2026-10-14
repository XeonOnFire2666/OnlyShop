using OnlyShop.Core.Common;
using OnlyShop.Core.Enums;
using OnlyShop.Core.ValueObjects;

namespace OnlyShop.Core.Entities;

public class Order : BaseEntity
{
    private readonly List<OrderItem> _items = [];
    private readonly List<OrderStatusChange> _statusHistory = [];

    private Order()
    {
    }

    public Guid CustomerId { get; private set; }
    public string CustomerName { get; private set; } = null!;
    public string? CustomerPhone { get; private set; }
    public OrderStatus Status { get; private set; }
    public Currency Currency { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public DeliveryMethod DeliveryMethod { get; private set; }
    public string? DeliveryAddress { get; private set; }
    public string? Comment { get; private set; }
    public decimal BonusesSpent { get; private set; }

    public IReadOnlyList<OrderItem> Items => _items;
    public IReadOnlyList<OrderStatusChange> StatusHistory => _statusHistory;

    public decimal ItemsTotal => _items.Sum(i => i.Total);
    public decimal Total => ItemsTotal - BonusesSpent;

    public static Order Create(
        Guid customerId,
        string customerName,
        string? customerPhone,
        Currency currency,
        PaymentMethod paymentMethod,
        DeliveryMethod deliveryMethod,
        string? deliveryAddress,
        string? comment,
        IEnumerable<NewOrderItem> items)
    {
        if (string.IsNullOrWhiteSpace(customerName))
        {
            throw new DomainException("Customer name is required");
        }

        if (deliveryMethod != DeliveryMethod.Pickup && string.IsNullOrWhiteSpace(deliveryAddress))
        {
            throw new DomainException("Delivery address is required");
        }

        var order = new Order
        {
            CustomerId = customerId,
            CustomerName = customerName.Trim(),
            CustomerPhone = customerPhone,
            Currency = currency,
            PaymentMethod = paymentMethod,
            DeliveryMethod = deliveryMethod,
            DeliveryAddress = deliveryAddress,
            Comment = comment,
            Status = OrderStatus.Pending
        };

        foreach (var item in items)
        {
            order._items.Add(OrderItem.Create(order.Id, item));
        }

        if (order._items.Count == 0)
        {
            throw new DomainException("Order must contain at least one item");
        }

        return order;
    }

    public void ApplyBonuses(decimal amount)
    {
        if (Status != OrderStatus.Pending)
        {
            throw new DomainException("Bonuses can only be applied to a pending order");
        }

        if (amount < 0)
        {
            throw new DomainException("Bonus amount cannot be negative");
        }

        if (amount > ItemsTotal)
        {
            throw new DomainException("Bonuses cannot exceed the order total");
        }

        BonusesSpent = amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeStatus(OrderStatus newStatus, Guid? staffId = null, string? note = null)
    {
        if (!CanTransitionTo(newStatus))
        {
            throw new DomainException($"Cannot change order status from {Status} to {newStatus}");
        }

        _statusHistory.Add(OrderStatusChange.Create(Id, Status, newStatus, staffId, note));
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool CanTransitionTo(OrderStatus target) => (Status, target) switch
    {
        (OrderStatus.Pending, OrderStatus.Confirmed) => true,
        (OrderStatus.Pending, OrderStatus.Canceled) => true,
        (OrderStatus.Confirmed, OrderStatus.Paid) => true,
        (OrderStatus.Confirmed, OrderStatus.Canceled) => true,
        (OrderStatus.Paid, OrderStatus.Delivering) => true,
        (OrderStatus.Paid, OrderStatus.Canceled) => true,
        (OrderStatus.Delivering, OrderStatus.Completed) => true,
        _ => false
    };
}