using OnlyShop.Application.Models;
using OnlyShop.Core.Enums;

namespace OnlyShop.Application.Dtos;

public sealed record CategoryDto(Guid Id, string Name, string Slug, Guid? ParentId, int SortOrder);

public sealed record PresaleLocationDto(Guid Id, string Name, string? Description, string? PhotoUrl);

public sealed record ProductVariantDto(Guid Id, string Size, int StockQuantity, bool IsAvailable);

public sealed record ProductCardDto(
    Guid Id,
    string Name,
    string? Brand,
    Guid CategoryId,
    ProductCondition Condition,
    decimal PriceByn,
    decimal PriceRub,
    ProductAvailability Availability,
    DateTime? PresaleClosesAt,
    bool CanBeOrdered,
    string? CoverUrl,
    IReadOnlyList<string> AvailableSizes);

public sealed record ProductDetailsDto(
    Guid Id,
    string Name,
    string? Brand,
    string? Description,
    Guid CategoryId,
    ProductCondition Condition,
    decimal PriceByn,
    decimal PriceRub,
    ProductAvailability Availability,
    PresaleLocationDto? PresaleLocation,
    DateTime? PresaleClosesAt,
    bool CanBeOrdered,
    IReadOnlyList<string> PhotoUrls,
    IReadOnlyList<ProductVariantDto> Variants);

public sealed record CatalogQuery(
    Guid? CategoryId = null,
    string? Search = null,
    ProductAvailability? Availability = null,
    ProductSort Sort = ProductSort.Newest,
    int Page = 1,
    int PageSize = 20);