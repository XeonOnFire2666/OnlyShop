using OnlyShop.Core.Enums;

namespace OnlyShop.Application.Dtos;

public sealed record StaffDto(
    Guid Id,
    string Email,
    string Name,
    StaffRole Role,
    bool IsActive,
    long? TelegramId);

public sealed record CreateStaffRequest(string Email, string Password, string Name, StaffRole Role, long? TelegramId = null);

public sealed record UpdateStaffTelegramRequest(long? TelegramId);