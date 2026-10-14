using Microsoft.Extensions.Logging;
using OnlyShop.Application.Abstractions.Infrastructure;
using OnlyShop.Application.Abstractions.Persistence;
using OnlyShop.Application.Dtos;

namespace OnlyShop.Application.Services;

internal sealed class OrderNotificationService(
    IStaffRepository staff,
    IOrderNotifier notifier,
    ILogger<OrderNotificationService> logger)
{
    public async Task NotifyNewOrderAsync(OrderDto order, UserDto customer, CancellationToken ct)
    {
        try
        {
            var recipients = await staff.GetNotificationTelegramIdsAsync(ct);

            if (recipients.Count > 0)
            {
                await notifier.NotifyManagersAboutNewOrderAsync(order, customer, recipients, ct);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Failed to notify managers about order {OrderId}", order.Id);
        }
    }

    public async Task NotifyStatusChangedAsync(long customerTelegramId, OrderDto order, CancellationToken ct)
    {
        try
        {
            await notifier.NotifyCustomerAboutStatusAsync(customerTelegramId, order, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Failed to notify customer about order {OrderId}", order.Id);
        }
    }
}