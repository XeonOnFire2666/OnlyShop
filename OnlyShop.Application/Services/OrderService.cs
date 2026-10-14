using Microsoft.Extensions.Options;
using OnlyShop.Application.Abstractions.Persistence;
using OnlyShop.Application.Abstractions.Services;
using OnlyShop.Application.Common;
using OnlyShop.Application.Common.Exceptions;
using OnlyShop.Application.Common.Models;
using OnlyShop.Application.Dtos;
using OnlyShop.Application.Mapping;
using OnlyShop.Application.Models;
using OnlyShop.Application.Options;
using OnlyShop.Core.Common;
using OnlyShop.Core.Entities;
using OnlyShop.Core.Enums;
using OnlyShop.Core.ValueObjects;

namespace OnlyShop.Application.Services;

internal sealed class OrderService(
    IUserRepository users,
    IBasketRepository baskets,
    IProductRepository products,
    IOrderRepository orders,
    IUnitOfWork uow,
    OrderWorkflow workflow,
    OrderNotificationService notifications,
    IOptions<BonusOptions> bonusOptions) : IOrderService
{
    public async Task<OrderDto> CheckoutAsync(Guid userId, CheckoutRequest request, CancellationToken ct = default)
    {
        var user = await users.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException(nameof(User), userId);

        var basket = await baskets.GetByUserIdAsync(userId, ct);

        if (basket is null || basket.Items.Count == 0)
        {
            throw new DomainException("Basket is empty");
        }

        var phone = string.IsNullOrWhiteSpace(request.Phone) ? user.Phone : request.Phone.Trim();

        if (phone is null && user.Username is null)
        {
            throw new DomainException("Contact phone is required");
        }

        if (phone != user.Phone)
        {
            user.UpdateContact(user.Name, user.Username, phone);
        }

        var ids = basket.Items.Select(i => i.ProductId).Distinct().ToList();
        var byId = (await products.GetByIdsAsync(ids, ct)).ToDictionary(p => p.Id);
        var orderItems = new List<NewOrderItem>();

        foreach (var item in basket.Items)
        {
            var product = byId.GetValueOrDefault(item.ProductId)
                ?? throw new NotFoundException(nameof(Product), item.ProductId);

            var variant = product.Variants.FirstOrDefault(v => v.Id == item.ProductVariantId)
                ?? throw new DomainException($"Selected size of {product.Name} is no longer available");

            product.ReserveStock(variant.Id, item.Quantity);

            orderItems.Add(new NewOrderItem(
                product.Id,
                product.Name,
                variant.Size,
                product.Price.In(request.Currency),
                item.Quantity));
        }

        var order = Order.Create(
            user.Id,
            user.Name,
            phone,
            request.Currency,
            request.PaymentMethod,
            request.DeliveryMethod,
            request.DeliveryAddress,
            request.Comment,
            orderItems);

        if (request.BonusesToSpend != 0)
        {
            var maxBonuses = Math.Round(order.ItemsTotal * bonusOptions.Value.MaxSpendPercent / 100m, 2);

            if (request.BonusesToSpend > maxBonuses)
            {
                throw new DomainException($"Bonuses can cover at most {maxBonuses} of this order");
            }

            user.SpendBonuses(request.BonusesToSpend);
            order.ApplyBonuses(request.BonusesToSpend);
        }

        basket.Clear();
        orders.Add(order);
        await uow.SaveChangesAsync(ct);

        var dto = order.ToDto();
        await notifications.NotifyNewOrderAsync(dto, user.ToDto(), ct);

        return dto;
    }

    public async Task<PagedResult<OrderDto>> GetMyOrdersAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var (normalizedPage, normalizedSize) = PagingRules.Normalize(page, pageSize);
        var criteria = new OrderSearchCriteria(null, userId, null, null, null, normalizedPage, normalizedSize);

        var result = await orders.SearchAsync(criteria, ct);
        return result.Map(o => o.ToDto());
    }

    public async Task<OrderDto> GetMyOrderAsync(Guid userId, Guid orderId, CancellationToken ct = default)
    {
        var order = await GetOwnedAsync(userId, orderId, ct);
        return order.ToDto();
    }

    public async Task<OrderDto> CancelMyOrderAsync(Guid userId, Guid orderId, CancellationToken ct = default)
    {
        var order = await GetOwnedAsync(userId, orderId, ct);

        if (order.Status != OrderStatus.Pending)
        {
            throw new DomainException("Only a pending order can be canceled. Please contact a manager");
        }

        await workflow.ChangeStatusAsync(order, OrderStatus.Canceled, null, "Canceled by customer", ct);
        return order.ToDto();
    }

    private async Task<Order> GetOwnedAsync(Guid userId, Guid orderId, CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(orderId, ct);

        if (order is null || order.CustomerId != userId)
        {
            throw new NotFoundException(nameof(Order), orderId);
        }

        return order;
    }
}