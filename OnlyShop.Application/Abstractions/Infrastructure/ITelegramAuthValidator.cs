using OnlyShop.Application.Models;

namespace OnlyShop.Application.Abstractions.Infrastructure;

public interface ITelegramAuthValidator
{
    TelegramUserData? Validate(string initData);
}