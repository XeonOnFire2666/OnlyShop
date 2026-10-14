using OnlyShop.Application.Dtos;

namespace OnlyShop.Application.Abstractions.Services;

public interface IUserService
{
    Task<UserDto> GetAsync(Guid userId, CancellationToken ct = default);
    Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default);
}