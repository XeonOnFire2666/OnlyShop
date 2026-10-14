using OnlyShop.Application.Models;
using OnlyShop.Core.Enums;

namespace OnlyShop.Application.Dtos;

public sealed record VariantInput(string Size, int StockQuantity);

public sealed record CreateProductRequest(
    string Name,
    Guid CategoryId,
    decimal PriceByn,
    decimal PriceRub,
    ProductCondition Condition,
    string? Brand,
    string? Description,
    IReadOnlyList<VariantInput>? Variants);

public sealed record UpdateProductRequest(
    string Name,
    string? Brand,
    string? Description,
    ProductCondition Condition);

public sealed record ChangePriceRequest(decimal PriceByn, decimal PriceRub);

public sealed record SetPresaleRequest(Guid LocationId, DateTime ClosesAt);

public sealed record AdminVariantDto(Guid Id, string Size, int StockQuantity);

public sealed record AdminPhotoDto(Guid Id, string Url, int SortOrder);

public sealed record AdminProductDto(
    Guid Id,
    string Name,
    string? Brand,
    string? Description,
    Guid CategoryId,
    ProductCondition Condition,
    decimal PriceByn,
    decimal PriceRub,
    ProductAvailability Availability,
    ProductStatus Status,
    Guid? PresaleLocationId,
    DateTime? PresaleClosesAt,
    Guid CreatedById,
    Guid UpdatedById,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<AdminPhotoDto> Photos,
    IReadOnlyList<AdminVariantDto> Variants);

public sealed record AdminProductListItemDto(
    Guid Id,
    string Name,
    string? Brand,
    Guid CategoryId,
    ProductStatus Status,
    ProductAvailability Availability,
    decimal PriceByn,
    decimal PriceRub,
    int TotalStock,
    string? CoverUrl,
    DateTime UpdatedAt);

public sealed record AdminProductQuery(
    Guid? CategoryId = null,
    string? Search = null,
    ProductStatus? Status = null,
    ProductAvailability? Availability = null,
    ProductSort Sort = ProductSort.Newest,
    int Page = 1,
    int PageSize = 20);

public sealed record CategoryRequest(string Name, string Slug, Guid? ParentId, int SortOrder);

public sealed record PresaleLocationRequest(string Name, string? Description);