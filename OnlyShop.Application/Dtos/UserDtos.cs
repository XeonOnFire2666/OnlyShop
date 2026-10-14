namespace OnlyShop.Application.Dtos;

public sealed record UserDto(
    Guid Id,
    long TelegramId,
    string Name,
    string? Username,
    string? Phone,
    decimal BonusBalance);

public sealed record UpdateProfileRequest(string Name, string? Phone);