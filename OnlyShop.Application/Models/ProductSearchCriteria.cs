using OnlyShop.Core.Enums;

namespace OnlyShop.Application.Models;

public enum ProductSort
{
    Newest,
    PriceAsc,
    PriceDesc
}

public sealed record ProductSearchCriteria(
    IReadOnlyCollection<Guid>? CategoryIds,
    string? Search,
    ProductAvailability? Availability,
    ProductStatus? Status,
    bool ExcludeClosedPresales,
    ProductSort Sort,
    int Page,
    int PageSize);