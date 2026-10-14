using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OnlyShop.Adapters.Options;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace OnlyShop.Adapters.Telegram;

internal static class TelegramWebhookEndpoint
{
    private const string SecretHeader = "X-Telegram-Bot-Api-Secret-Token";

    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/telegram/webhook", async (
            HttpRequest request,
            IOptions<TelegramWebhookOptions> options,
            TelegramUpdateHandler handler,
            CancellationToken ct) =>
        {
            var provided = Encoding.UTF8.GetBytes(request.Headers[SecretHeader].ToString());
            var expected = Encoding.UTF8.GetBytes(options.Value.SecretToken);

            if (!CryptographicOperations.FixedTimeEquals(provided, expected))
            {
                return Results.Unauthorized();
            }

            var update = await request.ReadFromJsonAsync<Update>(JsonBotAPI.Options, ct);

            if (update is not null)
            {
                await handler.HandleAsync(update, ct);
            }

            return Results.Ok();
        }).AllowAnonymous();
}
