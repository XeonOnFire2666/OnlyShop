using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OnlyShop.Adapters.Options;
using OnlyShop.Application.Abstractions.Persistence;
using OnlyShop.Application.Abstractions.Services;
using OnlyShop.Application.Dtos;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace OnlyShop.Adapters.Telegram;

internal sealed class TelegramUpdateHandler(
    ITelegramBotClient bot,
    IStaffService staff,
    IOrderManagementService orders,
    IOrderRepository orderRepo,
    OrderNotifier notifier,
    StaffLinkCodes linkCodes,
    IMemoryCache cache,    
    IOptions<TelegramWebhookOptions> options,
    ILogger<TelegramUpdateHandler> logger)
{
    public async Task HandleAsync(Update update, CancellationToken ct)
    {
        try
        {
            if (update.CallbackQuery is { Data: { } data } callback)
            {
                await HandleCallbackAsync(callback, data, ct);
                return;
            }

            if (update.Message is { Photo: { } photos, From: { } userFrom } photoMsg && photoMsg.Chat.Type == ChatType.Private)
            {
                await HandlePhotoAsync(photoMsg, photos[^1].FileId, userFrom, ct);
                return;
            }

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

            if (payload.StartsWith("pay_"))
            {
                var orderIdStr = payload[4..];
                if (Guid.TryParse(orderIdStr, out var orderId))
                {
                    cache.Set($"pay_{from.Id}", orderId, TimeSpan.FromMinutes(30));
                    await bot.SendMessage(message.Chat.Id, "💳 Пожалуйста, отправьте скриншот чека об оплате прямо в этот чат.", cancellationToken: ct);
                    return;
                }
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

    private async Task HandlePhotoAsync(Message msg, string fileId, User from, CancellationToken ct)
    {
        if (cache.TryGetValue($"pay_{from.Id}", out Guid orderId))
        {
            cache.Remove($"pay_{from.Id}");   

            var allStaff = await staff.GetAllAsync(ct);
            var managerIds = allStaff.Where(s => s.IsActive && s.TelegramId.HasValue).Select(s => s.TelegramId!.Value).ToList();

            if (!managerIds.Any())
            {
                await bot.SendMessage(msg.Chat.Id, "Ошибка: нет активных менеджеров. Пожалуйста, свяжитесь с администратором.", cancellationToken: ct);
                return;
            }

            var caption = $"🧾 <b>Чек об оплате</b>\nЗаказ: #{orderId.ToString()[..8].ToUpper()}\nКлиент: {from.FirstName} (@{from.Username})\nID клиента: {from.Id}";

            foreach (var managerId in managerIds)
            {
                try
                {
                    await bot.SendPhoto(managerId, fileId, caption: caption, parseMode: ParseMode.Html, cancellationToken: ct);
                }
                catch {       }
            }

            await bot.SendMessage(msg.Chat.Id, "✅ Спасибо! Скриншот отправлен менеджеру. Ожидайте подтверждения оплаты.", cancellationToken: ct);
        }
        else
        {
            await bot.SendMessage(msg.Chat.Id, "Не понял, к какому заказу относится это фото. Пожалуйста, вернитесь в Mini App и нажмите кнопку «Отправить чек».", cancellationToken: ct);
        }
    }

    private async Task HandleCallbackAsync(CallbackQuery callback, string data, CancellationToken ct)
    {
        if (!OrderKeyboard.TryParse(data, out var orderId, out var newStatus))
            return;

        try
        {
            await orders.ChangeStatusAsync(null, orderId, new ChangeOrderStatusRequest(newStatus, "Изменено из Telegram"), ct);
            await bot.AnswerCallbackQuery(callback.Id, $"Статус изменён на: {newStatus}", cancellationToken: ct);

            if (callback.Message is { } msg)
            {
                var order = await orderRepo.GetByIdAsync(orderId, ct);
                if (order is not null)
                {
                    await notifier.RefreshAsync(order, number: 0, username: null, chatId: msg.Chat.Id, messageId: msg.MessageId, ct);
                }
            }
        }
        catch (Exception ex)
        {
            await bot.AnswerCallbackQuery(callback.Id, $"Ошибка: {ex.Message}", showAlert: true, cancellationToken: ct);
            logger.LogWarning(ex, "Failed to change order status via telegram for {OrderId}", orderId);
        }
    }
}