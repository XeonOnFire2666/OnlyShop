using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OnlyShop.Adapters.Common;
using OnlyShop.Adapters.Endpoints;
using OnlyShop.Adapters.Options;
using OnlyShop.Adapters.Telegram;
using OnlyShop.Infrastructure.Jwt;

namespace OnlyShop.Adapters;

public static class DependencyInjection
{
    private const string SmartScheme = "SmartScheme";

    public static IServiceCollection AddAdapters(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(o => o.Key is { Length: >= 32 }, "Jwt:Secret must be at least 32 characters")
            .ValidateOnStart();

        services.AddOptions<TelegramWebhookOptions>()
            .Bind(configuration.GetSection(TelegramWebhookOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.SecretToken)
                           && !string.IsNullOrWhiteSpace(o.MiniAppUrl)
                           && !string.IsNullOrWhiteSpace(o.BotUsername),
                "TelegramWebhook: SecretToken, MiniAppUrl and BotUsername are required")
            .ValidateOnStart();

        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("Jwt section is missing");

        services.AddMemoryCache();
        services.AddSingleton<StaffLinkCodes>();
        services.AddScoped<TelegramUpdateHandler>();
        services.AddHostedService<WebhookRegistrationService>();

        services.AddExceptionHandler<ApiExceptionHandler>();
        services.AddProblemDetails();
        services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        // --- Authentication: JWT для staff/admin, Telegram initData для клиентов ---
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = SmartScheme;
            options.DefaultChallengeScheme = SmartScheme;
        })
        .AddPolicyScheme(SmartScheme, SmartScheme, options =>
        {
            // Если заголовок начинается с "tma " — идём в Telegram-схему,
            // иначе — в JWT (staff/admin).
            options.ForwardDefaultSelector = context =>
            {
                var auth = context.Request.Headers.Authorization.ToString();
                return auth.StartsWith("tma ", StringComparison.OrdinalIgnoreCase)
                    ? TelegramInitDataAuthenticationHandler.Scheme
                    : JwtBearerDefaults.AuthenticationScheme;
            };
        })
        .AddJwtBearer(o =>
        {
            o.MapInboundClaims = false;
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = jwt.Issuer,
                ValidAudience = jwt.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                NameClaimType = "sub",
                RoleClaimType = "role",
                ClockSkew = TimeSpan.FromSeconds(30)
            };
        })
        .AddScheme<AuthenticationSchemeOptions, TelegramInitDataAuthenticationHandler>(
            TelegramInitDataAuthenticationHandler.Scheme, _ => { });

        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.Customer, p => p.RequireRole(Roles.Customer))
            .AddPolicy(Policies.Staff, p => p.RequireRole(Roles.Manager, Roles.Admin))
            .AddPolicy(Policies.AdminOnly, p => p.RequireRole(Roles.Admin));

        services.AddRateLimiter(o =>
        {
            o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            o.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) }));
        });

        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        services.AddCors(o => o.AddDefaultPolicy(p => p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod()));

        return services;
    }

    public static IEndpointRouteBuilder MapAdapters(this IEndpointRouteBuilder app)
    {
        TelegramWebhookEndpoint.Map(app);
        AuthEndpoints.Map(app);
        MiniAppEndpoints.Map(app);
        AdminEndpoints.Map(app);
        return app;
    }
}