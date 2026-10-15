using Microsoft.Extensions.Logging;
using OnlyShop.Core.Entities;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace OnlyShop.Adapters.Telegram;

internal sealed class OrderNotifier(ITelegramBotClient bot, ILogger<OrderNotifier> logger)
{
    public async Task NotifyNewOrderAsync(
        Order order,
        long number,
        string? username,
        IEnumerable<long> managerChatIds,
        CancellationToken ct)
    {
        var text = OrderMessageFormatter.Format(order, number, username);
        var keyboard = OrderKeyboard.Build(order.Id);

        foreach (var chatId in managerChatIds)
        {
            try
            {
                await bot.SendMessage(chatId, text, ParseMode.Html, replyMarkup: keyboard, cancellationToken: ct);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to notify manager {ChatId} about order {OrderId}", chatId, order.Id);
            }
        }
    }

    public Task RefreshAsync(
        Order order,
        long number,
        string? username,
        long chatId,
        int messageId,
        CancellationToken ct) =>
        bot.EditMessageText(
            chatId,
            messageId,
            OrderMessageFormatter.Format(order, number, username).Replace("Новый заказ", "Заказ"),
            ParseMode.Html,
            replyMarkup: OrderKeyboard.Build(order.Id),
            cancellationToken: ct);
}