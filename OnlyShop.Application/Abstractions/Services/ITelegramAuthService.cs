using OnlyShop.Application.Dtos;

namespace OnlyShop.Application.Abstractions.Services;

public interface ITelegramAuthService
{
    Task<UserDto> AuthenticateAsync(string initData, CancellationToken ct = default);
}