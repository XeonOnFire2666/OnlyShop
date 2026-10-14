using OnlyShop.Application.Dtos;

namespace OnlyShop.Adapters.Common;

internal sealed record TelegramLoginRequest(string? InitData);

internal sealed record StaffLoginRequest(string? Email, string? Password);

internal sealed record CustomerAuthResponse(string Token, UserDto User);

internal sealed record StaffAuthResponse(string Token, StaffDto Staff);

internal sealed record QuantityRequest(int Quantity);

internal sealed record StockDeltaRequest(int Delta);

internal sealed record SetActiveRequest(bool IsActive);
