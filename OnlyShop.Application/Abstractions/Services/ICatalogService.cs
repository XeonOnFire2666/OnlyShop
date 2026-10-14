using OnlyShop.Application.Common.Models;
using OnlyShop.Application.Dtos;

namespace OnlyShop.Application.Abstractions.Services;

public interface ICatalogService
{
    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken ct = default);
    Task<PagedResult<ProductCardDto>> SearchAsync(CatalogQuery query, CancellationToken ct = default);
    Task<ProductDetailsDto> GetProductAsync(Guid productId, CancellationToken ct = default);
}