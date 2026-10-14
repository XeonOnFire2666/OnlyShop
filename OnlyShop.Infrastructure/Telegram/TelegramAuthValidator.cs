using System.Security.Cryptography;
using System.Text;
using System.Web;
using Microsoft.Extensions.Options;
using OnlyShop.Application.Abstractions.Infrastructure;
using OnlyShop.Application.Models;

namespace OnlyShop.Infrastructure.Telegram;

public class TelegramAuthValidator(IOptions<TelegramOptions> options) : ITelegramAuthValidator
{
    private readonly string _botToken = options.Value.BotToken;

    public TelegramUserData? Validate(string initData)
    {
        var parsed = HttpUtility.ParseQueryString(initData);
        var hash = parsed["hash"];
        if (string.IsNullOrEmpty(hash))
        {
            return null;
        }

        var pairs = parsed.AllKeys
            .Where(k => k != "hash" && k is not null)
            .Select(k => $"{k}={parsed[k]}")
            .OrderBy(x => x, StringComparer.Ordinal);

        var dataCheckString = string.Join('\n', pairs);

        var secretKey = HMACSHA256.HashData(Encoding.UTF8.GetBytes("WebAppData"), Encoding.UTF8.GetBytes(_botToken));
        var computedHash = Convert.ToHexString(HMACSHA256.HashData(secretKey, Encoding.UTF8.GetBytes(dataCheckString))).ToLowerInvariant();

        if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(computedHash), Encoding.UTF8.GetBytes(hash)))
        {
            return null;
        }

        var userJson = parsed["user"];
        if (string.IsNullOrEmpty(userJson))
        {
            return null;
        }

        using var doc = System.Text.Json.JsonDocument.Parse(userJson);
        var root = doc.RootElement;

        return new TelegramUserData(
            root.GetProperty("id").GetInt64(),
            root.GetProperty("first_name").GetString() ?? "",
            root.TryGetProperty("last_name", out var ln) ? ln.GetString() : null,
            root.TryGetProperty("username", out var un) ? un.GetString() : null);
    }
}