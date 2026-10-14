namespace OnlyShop.Application.Common;

internal static class PagingRules
{
    public static (int Page, int PageSize) Normalize(int page, int pageSize, int maxPageSize = 50) =>
        (Math.Max(page, 1), Math.Clamp(pageSize, 1, maxPageSize));
}