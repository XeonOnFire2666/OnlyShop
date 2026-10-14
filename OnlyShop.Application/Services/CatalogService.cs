using OnlyShop.Application.Abstractions.Persistence;
using OnlyShop.Application.Abstractions.Services;
using OnlyShop.Application.Common;
using OnlyShop.Application.Common.Exceptions;
using OnlyShop.Application.Common.Models;
using OnlyShop.Application.Dtos;
using OnlyShop.Application.Mapping;
using OnlyShop.Application.Models;
using OnlyShop.Core.Entities;
using OnlyShop.Core.Enums;

namespace OnlyShop.Application.Services;

internal sealed class CatalogService(
    IProductRepository products,
    ICategoryRepository categories,
    IPresaleLocationRepository locations,
    ProductMapper mapper) : ICatalogService
{
    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken ct = default)
    {
        var all = await categories.GetAllAsync(ct);

        return all
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .Select(c => c.ToDto())
            .ToList();
    }

    public async Task<PagedResult<ProductCardDto>> SearchAsync(CatalogQuery query, CancellationToken ct = default)
    {
        var (page, pageSize) = PagingRules.Normalize(query.Page, query.PageSize);

        IReadOnlyCollection<Guid>? categoryIds = null;

        if (query.CategoryId is { } categoryId)
        {
            var all = await categories.GetAllAsync(ct);
            categoryIds = CategoryTree.GetSelfAndDescendantIds(all, categoryId);
        }

        var criteria = new ProductSearchCriteria(
            categoryIds,
            query.Search?.Trim(),
            query.Availability,
            ProductStatus.Published,
            true,
            query.Sort,
            page,
            pageSize);

        var result = await products.SearchAsync(criteria, ct);
        return result.Map(mapper.ToCard);
    }

    public async Task<ProductDetailsDto> GetProductAsync(Guid productId, CancellationToken ct = default)
    {
        var product = await products.GetByIdAsync(productId, ct);

        if (product is null || !product.IsPublished)
        {
            throw new NotFoundException(nameof(Product), productId);
        }

        PresaleLocationDto? location = null;

        if (product.PresaleLocationId is { } locationId
            && await locations.GetByIdAsync(locationId, ct) is { } entity)
        {
            location = mapper.ToLocationDto(entity);
        }

        return mapper.ToDetails(product, location);
    }
}