using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnlyShop.Application.Abstractions.Services;
using OnlyShop.Application.Mapping;
using OnlyShop.Application.Options;
using OnlyShop.Application.Services;

namespace OnlyShop.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<BonusOptions>()
            .Bind(configuration.GetSection(BonusOptions.SectionName))
            .Validate(o => o.AccrualPercent is >= 0 and <= 100,
                "BonusOptions.AccrualPercent must be in [0, 100].")
            .Validate(o => o.MaxSpendPercent is >= 0 and <= 100,
                "BonusOptions.MaxSpendPercent must be in [0, 100].")
            .ValidateOnStart();

        services.AddScoped<ProductMapper>();
        services.AddScoped<OrderNotificationService>();
        services.AddScoped<OrderWorkflow>();

        services.AddScoped<ITelegramAuthService, TelegramAuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<IBasketService, BasketService>();
        services.AddScoped<IOrderService, OrderService>();

        services.AddScoped<IProductManagementService, ProductManagementService>();
        services.AddScoped<ICategoryManagementService, CategoryManagementService>();
        services.AddScoped<IPresaleLocationService, PresaleLocationService>();
        services.AddScoped<IOrderManagementService, OrderManagementService>();
        services.AddScoped<IStaffService, StaffService>();

        return services;
    }
}