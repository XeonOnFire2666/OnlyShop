using OnlyShop.Application.Abstractions.Persistence;
using OnlyShop.Application.Abstractions.Services;
using OnlyShop.Application.Common;
using OnlyShop.Application.Common.Exceptions;
using OnlyShop.Application.Dtos;
using OnlyShop.Application.Mapping;
using OnlyShop.Core.Common;
using OnlyShop.Core.Entities;

namespace OnlyShop.Application.Services;

internal sealed class CategoryManagementService(
    ICategoryRepository categories,
    IProductRepository products,
    IUnitOfWork uow) : ICategoryManagementService
{
    public async Task<CategoryDto> CreateAsync(CategoryRequest request, CancellationToken ct = default)
    {
        var (name, slug) = await ValidateAsync(request, null, ct);

        var category = Category.Create(name, slug, request.ParentId, request.SortOrder);

        categories.Add(category);
        await uow.SaveChangesAsync(ct);

        return category.ToDto();
    }

    public async Task<CategoryDto> UpdateAsync(Guid categoryId, CategoryRequest request, CancellationToken ct = default)
    {
        var category = await categories.GetByIdAsync(categoryId, ct)
            ?? throw new NotFoundException(nameof(Category), categoryId);

        var (name, slug) = await ValidateAsync(request, categoryId, ct);

        category.Update(name, slug, request.ParentId, request.SortOrder);

        await uow.SaveChangesAsync(ct);
        return category.ToDto();
    }
    public async Task DeleteAsync(Guid categoryId, CancellationToken ct = default)
    {
        var category = await categories.GetByIdAsync(categoryId, ct)
            ?? throw new NotFoundException(nameof(Category), categoryId);

        var all = await categories.GetAllAsync(ct);

        if (all.Any(c => c.ParentId == categoryId))
        {
            throw new DomainException("Cannot delete a category that has subcategories");
        }

        if (await products.AnyInCategoryAsync(categoryId, ct))
        {
            throw new DomainException("Cannot delete a category that contains products");
        }

        categories.Remove(category);
        await uow.SaveChangesAsync(ct);
    }

    private async Task<(string Name, string Slug)> ValidateAsync(CategoryRequest request, Guid? currentId, CancellationToken ct)
    {
        var name = request.Name?.Trim();
        var slug = request.Slug?.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Category name is required");
        }

        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new DomainException("Category slug is required");
        }

        if (await categories.SlugExistsAsync(slug, currentId, ct))
        {
            throw new DomainException($"Slug '{slug}' is already used");
        }

        if (request.ParentId is { } parentId)
        {
            if (!await categories.ExistsAsync(parentId, ct))
            {
                throw new NotFoundException(nameof(Category), parentId);
            }

            if (currentId is { } id)
            {
                var all = await categories.GetAllAsync(ct);

                if (CategoryTree.GetSelfAndDescendantIds(all, id).Contains(parentId))
                {
                    throw new DomainException("A category cannot be moved inside itself");
                }
            }
        }

        return (name, slug);
    }
}