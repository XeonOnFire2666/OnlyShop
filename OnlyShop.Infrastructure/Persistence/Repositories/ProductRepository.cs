using Microsoft.EntityFrameworkCore;
using OnlyShop.Application.Abstractions.Persistence;
using OnlyShop.Application.Common.Models;
using OnlyShop.Application.Models;
using OnlyShop.Core.Entities;

namespace OnlyShop.Infrastructure.Persistence.Repositories;

public class ProductRepository(OnlyShopDbContext db) : IProductRepository
{
    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Products.Include(x => x.Variants).Include(x => x.Photos)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<Product>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default) =>
        await db.Products.Include(x => x.Variants).Include(x => x.Photos)
            .Where(x => ids.Contains(x.Id)).ToListAsync(ct);

    public async Task<PagedResult<Product>> SearchAsync(ProductSearchCriteria c, CancellationToken ct = default)
    {
        var query = db.Products.Include(x => x.Variants).Include(x => x.Photos).AsQueryable();

        if (c.CategoryIds is { Count: > 0 })
            query = query.Where(x => c.CategoryIds.Contains(x.CategoryId));

        if (!string.IsNullOrWhiteSpace(c.Search))
            query = query.Where(x => EF.Functions.ILike(x.Name, $"%{c.Search}%"));

        if (c.Availability is not null)
            query = query.Where(x => x.Availability == c.Availability);

        if (c.Status is not null)
            query = query.Where(x => x.Status == c.Status);

        if (c.ExcludeClosedPresales)
            query = query.Where(x =>
                x.Availability != OnlyShop.Core.Enums.ProductAvailability.Presale
                || x.PresaleClosesAt > DateTime.UtcNow);

        query = c.Sort switch
        {
            ProductSort.PriceAsc => query.OrderBy(x => x.Price.Byn),
            ProductSort.PriceDesc => query.OrderByDescending(x => x.Price.Byn),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };

        var total = await query.CountAsync(ct);
        var items = await query.Skip((c.Page - 1) * c.PageSize).Take(c.PageSize).ToListAsync(ct);

        return new PagedResult<Product>(items, c.Page, c.PageSize, total);
    }

    public Task<bool> AnyInCategoryAsync(Guid categoryId, CancellationToken ct = default) =>
        db.Products.AnyAsync(x => x.CategoryId == categoryId, ct);

    public Task<bool> AnyForPresaleLocationAsync(Guid locationId, CancellationToken ct = default) =>
        db.Products.AnyAsync(x => x.PresaleLocationId == locationId, ct);

    public void Add(Product product) => db.Products.Add(product);
}