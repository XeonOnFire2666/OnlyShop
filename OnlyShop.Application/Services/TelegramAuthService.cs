using OnlyShop.Application.Abstractions.Infrastructure;
using OnlyShop.Application.Abstractions.Persistence;
using OnlyShop.Application.Abstractions.Services;
using OnlyShop.Application.Common.Exceptions;
using OnlyShop.Application.Dtos;
using OnlyShop.Application.Mapping;
using OnlyShop.Core.Entities;

namespace OnlyShop.Application.Services;

internal sealed class TelegramAuthService(
    ITelegramAuthValidator validator,
    IUserRepository users,
    IUnitOfWork uow) : ITelegramAuthService
{
    public async Task<UserDto> AuthenticateAsync(string initData, CancellationToken ct = default)
    {
        var data = validator.Validate(initData)
            ?? throw new UnauthorizedException("Invalid Telegram init data");

        var name = string.Join(' ', new[] { data.FirstName, data.LastName }
            .Where(part => !string.IsNullOrWhiteSpace(part)));

        var user = await users.GetByTelegramIdAsync(data.Id, ct);

        if (user is null)
        {
            user = User.Create(data.Id, name, data.Username);
            users.Add(user);
            await uow.SaveChangesAsync(ct);
        }
        else if (user.Name != name || user.Username != data.Username)
        {
            user.UpdateContact(name, data.Username, user.Phone);
            await uow.SaveChangesAsync(ct);
        }

        return user.ToDto();
    }
}