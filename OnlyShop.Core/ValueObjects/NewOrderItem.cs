namespace OnlyShop.Core.ValueObjects;

public sealed record NewOrderItem(
    Guid ProductId,
    string ProductName,
    string Size,
    decimal UnitPrice,
    int Quantity);