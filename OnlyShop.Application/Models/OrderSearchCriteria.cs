using OnlyShop.Core.Enums;

namespace OnlyShop.Application.Models;

public sealed record OrderSearchCriteria(
    OrderStatus? Status,
    Guid? CustomerId,
    string? Search,
    DateTime? From,
    DateTime? To,
    int Page,
    int PageSize);