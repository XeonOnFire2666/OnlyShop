namespace OnlyShop.Adapters.Options;



public sealed class TelegramWebhookOptions
{
    public const string SectionName = "TelegramWebhook";

    public string? PublicBaseUrl { get; set; }
    public string SecretToken { get; set; } = null!;
    public string MiniAppUrl { get; set; } = null!;
    public string BotUsername { get; set; } = null!;
}
