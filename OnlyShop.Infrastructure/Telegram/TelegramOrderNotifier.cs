using System.Globalization;
using System.Text;
using Microsoft.Extensions.Options;
using OnlyShop.Application.Abstractions.Infrastructure;
using OnlyShop.Application.Dtos;
using Telegram.Bot;

namespace OnlyShop.Infrastructure.Telegram;

// Требует пакет Telegram.Bot
public class TelegramOrderNotifier(ITelegramBotClient bot) : IOrderNotifier
{
    public async Task NotifyManagersAboutNewOrderAsync(
        OrderDto order, UserDto customer, IReadOnlyList<long> managerTelegramIds, CancellationToken ct = default)
    {
        var text = BuildOrderMessage(order, customer);

        foreach (var managerId in managerTelegramIds)
        {
            try
            {
                await bot.SendMessage(managerId, text, cancellationToken: ct);
            }
            catch
            {
                // не роняем весь процесс из-за одного недоступного получателя
            }
        }
    }

    public Task NotifyCustomerAboutStatusAsync(long customerTelegramId, OrderDto order, CancellationToken ct = default) =>
        bot.SendMessage(customerTelegramId, $"Статус вашего заказа №{order.Id:N} изменён: {order.Status}", cancellationToken: ct);

    private static string BuildOrderMessage(OrderDto order, UserDto customer)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"🛒 Новый заказ №{order.Id:N}");
        sb.AppendLine($"Клиент: {order.CustomerName} ({customer.Username ?? customer.Phone ?? "без контакта"})");
        sb.AppendLine($"Телефон: {order.CustomerPhone ?? "—"}");
        sb.AppendLine($"Доставка: {order.DeliveryMethod}, адрес: {order.DeliveryAddress ?? "—"}");
        sb.AppendLine($"Оплата: {order.PaymentMethod}, валюта: {order.Currency}");
        sb.AppendLine();

        foreach (var item in order.Items)
        {
            sb.AppendLine($"• {item.ProductName} ({item.Size}) x{item.Quantity} = {item.Total.ToString(CultureInfo.InvariantCulture)}");
        }

        sb.AppendLine();
        sb.AppendLine($"Итого: {order.Total.ToString(CultureInfo.InvariantCulture)} (бонусами списано: {order.BonusesSpent})");

        if (!string.IsNullOrWhiteSpace(order.Comment))
        {
            sb.AppendLine($"Комментарий: {order.Comment}");
        }

        return sb.ToString();
    }
}