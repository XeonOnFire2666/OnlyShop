using OnlyShop.Application.Abstractions.Infrastructure;
using OnlyShop.Application.Abstractions.Persistence;
using OnlyShop.Application.Abstractions.Services;
using OnlyShop.Application.Common;
using OnlyShop.Application.Common.Exceptions;
using OnlyShop.Application.Common.Models;
using OnlyShop.Application.Dtos;
using OnlyShop.Application.Mapping;
using OnlyShop.Application.Models;
using OnlyShop.Core.Common;
using OnlyShop.Core.Entities;
using OnlyShop.Core.ValueObjects;

namespace OnlyShop.Application.Services;

internal sealed class ProductManagementService(
    IProductRepository products,
    ICategoryRepository categories,
    IPresaleLocationRepository locations,
    IFileStorage storage,
    ProductMapper mapper,
    IUnitOfWork uow) : IProductManagementService
{
    public async Task<PagedResult<AdminProductListItemDto>> SearchAsync(AdminProductQuery query, CancellationToken ct = default)
    {
        var (page, pageSize) = PagingRules.Normalize(query.Page, query.PageSize, 100);

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
            query.Status,
            false,
            query.Sort,
            page,
            pageSize);

        var result = await products.SearchAsync(criteria, ct);
        return result.Map(mapper.ToAdminListItem);
    }

    public async Task<AdminProductDto> GetAsync(Guid productId, CancellationToken ct = default) =>
        mapper.ToAdmin(await LoadAsync(productId, ct));

    public async Task<AdminProductDto> CreateAsync(Guid staffId, CreateProductRequest request, CancellationToken ct = default)
    {
        if (!await categories.ExistsAsync(request.CategoryId, ct))
        {
            throw new NotFoundException(nameof(Category), request.CategoryId);
        }

        var product = Product.Create(
            request.Name,
            request.CategoryId,
            new ProductPrice(request.PriceByn, request.PriceRub),
            request.Condition,
            staffId,
            request.Brand,
            request.Description);

        if (request.Variants is not null)
        {
            foreach (var variant in request.Variants)
            {
                product.AddVariant(variant.Size, variant.StockQuantity, staffId);
            }
        }

        products.Add(product);
        await uow.SaveChangesAsync(ct);

        return mapper.ToAdmin(product);
    }

    public async Task<AdminProductDto> UpdateDetailsAsync(Guid staffId, Guid productId, UpdateProductRequest request, CancellationToken ct = default)
    {
        var product = await LoadAsync(productId, ct);
        product.UpdateDetails(request.Name, request.Brand, request.Description, request.Condition, staffId);
        return await SaveAsync(product, ct);
    }

    public async Task<AdminProductDto> ChangePriceAsync(Guid staffId, Guid productId, ChangePriceRequest request, CancellationToken ct = default)
    {
        var product = await LoadAsync(productId, ct);
        product.ChangePrice(new ProductPrice(request.PriceByn, request.PriceRub), staffId);
        return await SaveAsync(product, ct);
    }

    public async Task<AdminProductDto> SetPresaleAsync(Guid staffId, Guid productId, SetPresaleRequest request, CancellationToken ct = default)
    {
        var product = await LoadAsync(productId, ct);

        if (await locations.GetByIdAsync(request.LocationId, ct) is null)
        {
            throw new NotFoundException(nameof(PresaleLocation), request.LocationId);
        }

        product.MarkAsPresale(request.LocationId, request.ClosesAt, staffId);
        return await SaveAsync(product, ct);
    }

    public async Task<AdminProductDto> SetInStockAsync(Guid staffId, Guid productId, CancellationToken ct = default)
    {
        var product = await LoadAsync(productId, ct);
        product.MarkAsInStock(staffId);
        return await SaveAsync(product, ct);
    }

    public async Task<AdminProductDto> AddVariantAsync(Guid staffId, Guid productId, VariantInput request, CancellationToken ct = default)
    {
        var product = await LoadAsync(productId, ct);
        product.AddVariant(request.Size, request.StockQuantity, staffId);
        return await SaveAsync(product, ct);
    }

    public async Task<AdminProductDto> AdjustStockAsync(Guid staffId, Guid productId, Guid variantId, int delta, CancellationToken ct = default)
    {
        var product = await LoadAsync(productId, ct);

        var variant = product.Variants.FirstOrDefault(v => v.Id == variantId)
            ?? throw new NotFoundException(nameof(ProductVariant), variantId);

        if (delta == 0)
        {
            throw new DomainException("Stock change cannot be zero");
        }

        if (delta > 0)
        {
            variant.Restock(delta);
        }
        else
        {
            variant.Reserve(-delta);
        }

        return await SaveAsync(product, ct);
    }

    public async Task<AdminProductDto> AddPhotoAsync(Guid staffId, Guid productId, FileUpload file, CancellationToken ct = default)
    {
        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException("Only image files are allowed");
        }

        var product = await LoadAsync(productId, ct);
        var path = await storage.SaveAsync(file, $"products/{product.Id}", ct);

        try
        {
            product.AddPhoto(path, staffId);

            var newPhoto = product.Photos.Last();
            uow.Add(newPhoto);

            await uow.SaveChangesAsync(ct);
        }
        catch
        {
            await storage.DeleteAsync(path, CancellationToken.None);
            throw;
        }

        return mapper.ToAdmin(product);
    }

    public async Task<AdminProductDto> PublishAsync(Guid staffId, Guid productId, CancellationToken ct = default)
    {
        var product = await LoadAsync(productId, ct);
        product.Publish(staffId);
        return await SaveAsync(product, ct);
    }

    public async Task<AdminProductDto> ArchiveAsync(Guid staffId, Guid productId, CancellationToken ct = default)
    {
        var product = await LoadAsync(productId, ct);
        product.Archive(staffId);
        return await SaveAsync(product, ct);
    }

    private async Task<Product> LoadAsync(Guid productId, CancellationToken ct) =>
        await products.GetByIdAsync(productId, ct)
            ?? throw new NotFoundException(nameof(Product), productId);

    private async Task<AdminProductDto> SaveAsync(Product product, CancellationToken ct)
    {
        await uow.SaveChangesAsync(ct);
        return mapper.ToAdmin(product);
    }
}