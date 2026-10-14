using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OnlyShop.Application.Abstractions.Services;
using OnlyShop.Application.Common.Exceptions;

namespace OnlyShop.Adapters.Telegram;

internal sealed class TelegramInitDataAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory loggerFactory,
    UrlEncoder encoder,
    ITelegramAuthService auth)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, loggerFactory, encoder)
{
    public const string Scheme = "TelegramInitData";
    private const string Prefix = "tma ";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var header = Request.Headers.Authorization.ToString();

        if (!header.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }

        var initData = header[Prefix.Length..].Trim();

        try
        {
            var user = await auth.AuthenticateAsync(initData, Context.RequestAborted);

            var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("telegram_id", user.TelegramId.ToString()),
                new Claim(ClaimTypes.Name, user.Name)
            ], Scheme);

            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme);
            return AuthenticateResult.Success(ticket);
        }
        catch (UnauthorizedException ex)
        {
            return AuthenticateResult.Fail(ex.Message);
        }
    }
}