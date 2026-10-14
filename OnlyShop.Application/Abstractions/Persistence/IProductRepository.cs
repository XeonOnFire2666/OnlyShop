using OnlyShop.Application.Common.Models;
using OnlyShop.Application.Models;
using OnlyShop.Core.Entities;

namespace OnlyShop.Application.Abstractions.Persistence;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default);
    Task<PagedResult<Product>> SearchAsync(ProductSearchCriteria criteria, CancellationToken ct = default);
    Task<bool> AnyInCategoryAsync(Guid categoryId, CancellationToken ct = default);
    Task<bool> AnyForPresaleLocationAsync(Guid locationId, CancellationToken ct = default);
    void Add(Product product);
}