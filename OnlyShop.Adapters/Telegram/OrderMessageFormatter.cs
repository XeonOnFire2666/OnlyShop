using System.Net;
using System.Text;
using OnlyShop.Core.Entities;
using OnlyShop.Core.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace OnlyShop.Adapters.Telegram;

internal static class OrderMessageFormatter
{
    public static string Format(Order order, long number, string? username)
    {
        var currency = order.Currency == Currency.Rub ? "RUB" : "BYN";
        var sb = new StringBuilder();

        sb.AppendLine($"🆕 <b>Новый заказ #{number:D6}</b>");
        sb.AppendLine();

        foreach (var item in order.Items)
        {
            sb.AppendLine($"Товар: {Encode(item.ProductName)}");
            sb.AppendLine($"Размер: {Encode(item.Size)}");
            sb.AppendLine($"Количество: {item.Quantity}");
            sb.AppendLine($"Сумма: {item.Total:0.##} {currency}");
            sb.AppendLine();
        }

        if (order.BonusesSpent > 0)
        {
            sb.AppendLine($"Списано бонусов: {order.BonusesSpent:0.##}");
        }

        sb.AppendLine($"<b>Итого: {order.Total:0.##} {currency}</b>");
        sb.AppendLine();

        sb.AppendLine($"Клиент: {(string.IsNullOrWhiteSpace(username) ? Encode(order.CustomerName) : "@" + Encode(username))}");
        sb.AppendLine($"Телефон: {Encode(order.CustomerPhone ?? "не указан")}");

        if (order.DeliveryMethod != DeliveryMethod.Pickup)
        {
            sb.AppendLine($"Адрес: {Encode(order.DeliveryAddress ?? "не указан")}");
        }

        sb.AppendLine();
        sb.AppendLine($"Доставка: {DeliveryLabel(order.DeliveryMethod)}");
        sb.AppendLine($"Время ожидания: {WaitLabel(order.DeliveryMethod)}");
        sb.AppendLine();
        sb.AppendLine($"Комментарий: {Encode(string.IsNullOrWhiteSpace(order.Comment) ? "—" : order.Comment)}");
        sb.AppendLine();
        sb.AppendLine($"Статус: {StatusLabel(order.Status)}");
        sb.Append($"Обновлено: {DateTime.UtcNow.AddHours(3):dd.MM.yyyy HH:mm}");

        return sb.ToString();
    }

    public static string StatusLabel(OrderStatus status) => status switch
    {
        OrderStatus.Pending => "Ожидает подтверждения",
        OrderStatus.Confirmed => "Принят",
        OrderStatus.Paid => "Оплачен",
        OrderStatus.Delivering => "В пути",
        OrderStatus.Completed => "Завершён",
        OrderStatus.Canceled => "Отменён",
        _ => status.ToString()
    };

    private static string DeliveryLabel(DeliveryMethod method) =>
        method == DeliveryMethod.Pickup ? "Самовывоз" : "Курьер";

    private static string WaitLabel(DeliveryMethod method) =>
        method == DeliveryMethod.Pickup ? "по договорённости" : "1–3 дня";

    private static string Encode(string value) => WebUtility.HtmlEncode(value);
}

internal static class OrderKeyboard
{
    private const string Prefix = "order";

    public static InlineKeyboardMarkup Build(Guid orderId)
    {
        var id = orderId.ToString("N");

        return new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                Button("✅ Принять", id, OrderStatus.Confirmed),
                Button("💳 Оплачен", id, OrderStatus.Paid)
            },
            new[]
            {
                Button("🚚 В пути", id, OrderStatus.Delivering),
                Button("🏁 Завершён", id, OrderStatus.Completed)
            },
            new[]
            {
                Button("❌ Отменить", id, OrderStatus.Canceled)
            }
        });
    }

    public static bool TryParse(string? data, out Guid orderId, out OrderStatus status)
    {
        orderId = Guid.Empty;
        status = default;

        var parts = data?.Split(':');

        return parts is [Prefix, var id, var rawStatus]
            && Guid.TryParseExact(id, "N", out orderId)
            && Enum.TryParse(rawStatus, out status);
    }

    private static InlineKeyboardButton Button(string text, string orderId, OrderStatus status) =>
        InlineKeyboardButton.WithCallbackData(text, $"{Prefix}:{orderId}:{(int)status}");
}