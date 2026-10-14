using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnlyShop.Application.Abstractions.Infrastructure;
using OnlyShop.Application.Abstractions.Persistence;
using OnlyShop.Infrastructure.FileStorage;
using OnlyShop.Infrastructure.Jwt;
using OnlyShop.Infrastructure.Persistence;
using OnlyShop.Infrastructure.Persistence.Repositories;
using OnlyShop.Infrastructure.Security;
using OnlyShop.Infrastructure.Telegram;
using Telegram.Bot;

namespace OnlyShop.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<OnlyShopDbContext>(opt =>
            opt.UseNpgsql(configuration.GetConnectionString("Default")));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<OnlyShopDbContext>());

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IStaffRepository, StaffRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IPresaleLocationRepository, PresaleLocationRepository>();
        services.AddScoped<IBasketRepository, BasketRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        services.Configure<FileStorageOptions>(configuration.GetSection(FileStorageOptions.SectionName));
        services.AddScoped<IFileStorage, LocalFileStorage>();

        services.AddScoped<IPasswordHasher, PasswordHasher>();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddScoped<IJwtProvider, JwtProvider>();

        services.Configure<TelegramOptions>(configuration.GetSection(TelegramOptions.SectionName));
        services.AddHttpClient("telegram_bot_client")
            .AddTypedClient<ITelegramBotClient>((http, sp) =>
            {
                var token = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<TelegramOptions>>().Value.BotToken;
                return new TelegramBotClient(token, http);
            });
        services.AddScoped<ITelegramAuthValidator, TelegramAuthValidator>();
        services.AddScoped<IOrderNotifier, TelegramOrderNotifier>();

        return services;
    }
}