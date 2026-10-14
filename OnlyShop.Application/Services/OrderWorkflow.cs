using Microsoft.Extensions.Options;
using OnlyShop.Application.Abstractions.Persistence;
using OnlyShop.Application.Common.Exceptions;
using OnlyShop.Application.Mapping;
using OnlyShop.Application.Options;
using OnlyShop.Core.Entities;
using OnlyShop.Core.Enums;

namespace OnlyShop.Application.Services;

internal sealed class OrderWorkflow(
    IUserRepository users,
    IProductRepository products,
    IUnitOfWork uow,
    OrderNotificationService notifications,
    IOptions<BonusOptions> bonusOptions)
{
    public async Task ChangeStatusAsync(
        Order order,
        OrderStatus newStatus,
        Guid? staffId,
        string? note,
        CancellationToken ct)
    {
        var customer = await users.GetByIdAsync(order.CustomerId, ct)
            ?? throw new NotFoundException(nameof(User), order.CustomerId);

        order.ChangeStatus(newStatus, staffId, note);

        switch (newStatus)
        {
            case OrderStatus.Canceled:
                await ReleaseOrderAsync(order, customer, ct);
                break;
            case OrderStatus.Completed:
                AccrueBonuses(order, customer);
                break;
        }

        await uow.SaveChangesAsync(ct);
        await notifications.NotifyStatusChangedAsync(customer.TelegramId, order.ToDto(), ct);
    }

    private async Task ReleaseOrderAsync(Order order, User customer, CancellationToken ct)
    {
        var ids = order.Items.Select(i => i.ProductId).Distinct().ToList();
        var byId = (await products.GetByIdsAsync(ids, ct)).ToDictionary(p => p.Id);

        foreach (var item in order.Items)
        {
            if (!byId.TryGetValue(item.ProductId, out var product))
            {
                continue;
            }

            var variant = product.Variants.FirstOrDefault(v =>
                string.Equals(v.Size, item.Size, StringComparison.OrdinalIgnoreCase));

            if (variant is not null)
            {
                product.ReleaseStock(variant.Id, item.Quantity);
            }
        }

        if (order.BonusesSpent > 0)
        {
            customer.AddBonuses(order.BonusesSpent);
        }
    }

    private void AccrueBonuses(Order order, User customer)
    {
        var bonus = Math.Round(order.Total * bonusOptions.Value.AccrualPercent / 100m, 2);

        if (bonus > 0)
        {
            customer.AddBonuses(bonus);
        }
    }
}