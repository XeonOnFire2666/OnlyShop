using OnlyShop.Application.Dtos;

namespace OnlyShop.Application.Abstractions.Infrastructure;

public interface IOrderNotifier
{
    Task NotifyManagersAboutNewOrderAsync(
        OrderDto order,
        UserDto customer,
        IReadOnlyList<long> managerTelegramIds,
        CancellationToken ct = default);

    Task NotifyCustomerAboutStatusAsync(
        long customerTelegramId,
        OrderDto order,
        CancellationToken ct = default);
}