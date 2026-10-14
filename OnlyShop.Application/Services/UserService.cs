using OnlyShop.Application.Abstractions.Persistence;
using OnlyShop.Application.Abstractions.Services;
using OnlyShop.Application.Common.Exceptions;
using OnlyShop.Application.Dtos;
using OnlyShop.Application.Mapping;
using OnlyShop.Core.Entities;

namespace OnlyShop.Application.Services;

internal sealed class UserService(IUserRepository users, IUnitOfWork uow) : IUserService
{
    public async Task<UserDto> GetAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await users.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException(nameof(User), userId);

        return user.ToDto();
    }

    public async Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var user = await users.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException(nameof(User), userId);

        var phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        user.UpdateContact(request.Name, user.Username, phone);

        await uow.SaveChangesAsync(ct);
        return user.ToDto();
    }
}