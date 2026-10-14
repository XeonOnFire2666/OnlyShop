using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OnlyShop.Adapters.Options;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace OnlyShop.Adapters.Telegram;

internal sealed class WebhookRegistrationService(
    ITelegramBotClient bot,
    IOptions<TelegramWebhookOptions> options,
    ILogger<WebhookRegistrationService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        var settings = options.Value;

        if (string.IsNullOrWhiteSpace(settings.PublicBaseUrl))
        {
            return;
        }

        try
        {
            await bot.SetWebhook(
                $"{settings.PublicBaseUrl.TrimEnd('/')}/telegram/webhook",
                allowedUpdates: [UpdateType.Message],
                secretToken: settings.SecretToken,
                cancellationToken: ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Telegram webhook registration failed");
        }
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
