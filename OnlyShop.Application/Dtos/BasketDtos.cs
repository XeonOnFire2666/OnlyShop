namespace OnlyShop.Application.Dtos;

public sealed record AddToBasketRequest(Guid ProductId, Guid VariantId, int Quantity = 1);

public sealed record BasketItemDto(
    Guid ProductId,
    Guid VariantId,
    string ProductName,
    string Size,
    string? PhotoUrl,
    decimal PriceByn,
    decimal PriceRub,
    int Quantity,
    bool IsAvailable);

public sealed record BasketDto(
    IReadOnlyList<BasketItemDto> Items,
    int TotalQuantity,
    decimal TotalByn,
    decimal TotalRub)
{
    public static BasketDto Empty { get; } = new([], 0, 0m, 0m);
}