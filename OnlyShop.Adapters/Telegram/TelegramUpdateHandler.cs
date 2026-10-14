using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OnlyShop.Adapters.Options;
using OnlyShop.Application.Abstractions.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace OnlyShop.Adapters.Telegram;

internal sealed class TelegramUpdateHandler(
    ITelegramBotClient bot,
    IStaffService staff,
    StaffLinkCodes linkCodes,
    IOptions<TelegramWebhookOptions> options,
    ILogger<TelegramUpdateHandler> logger)
{
    public async Task HandleAsync(Update update, CancellationToken ct)
    {
        try
        {
            if (update.Message is not { Text: { } text, From: { } from, Chat.Type: ChatType.Private } message)
            {
                return;
            }

            var parts = text.Split(' ', 2, StringSplitOptions.TrimEntries);

            if (!parts[0].StartsWith("/start"))
            {
                return;
            }

            var payload = parts.Length > 1 ? parts[1] : string.Empty;

            if (payload.StartsWith("link_") && linkCodes.TryConsume(payload[5..], out var staffId))
            {
                await staff.LinkTelegramAsync(staffId, from.Id, ct);
                await bot.SendMessage(message.Chat.Id, "Аккаунт привязан. Новые заказы будут приходить сюда.", cancellationToken: ct);
                return;
            }

            await bot.SendMessage(
                message.Chat.Id,
                "Добро пожаловать! Откройте каталог кнопкой ниже.",
                replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithWebApp("Открыть каталог", options.Value.MiniAppUrl)),
                cancellationToken: ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Failed to handle Telegram update {UpdateId}", update.Id);
        }
    }
}
