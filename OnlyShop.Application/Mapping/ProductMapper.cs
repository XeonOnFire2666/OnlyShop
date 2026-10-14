using OnlyShop.Application.Abstractions.Infrastructure;
using OnlyShop.Application.Dtos;
using OnlyShop.Core.Entities;

namespace OnlyShop.Application.Mapping;

internal sealed class ProductMapper(IFileStorage storage)
{
    public string? CoverUrl(Product product) =>
        product.Photos.OrderBy(p => p.SortOrder).FirstOrDefault() is { } photo
            ? storage.GetPublicUrl(photo.StoragePath)
            : null;

    public ProductCardDto ToCard(Product product) => new(
        product.Id,
        product.Name,
        product.Brand,
        product.CategoryId,
        product.Condition,
        product.Price.Byn,
        product.Price.Rub,
        product.Availability,
        product.PresaleClosesAt,
        product.CanBeOrdered,
        CoverUrl(product),
        product.Variants
            .Where(v => product.IsPresale || v.StockQuantity > 0)
            .Select(v => v.Size)
            .ToList());

    public ProductDetailsDto ToDetails(Product product, PresaleLocationDto? location) => new(
        product.Id,
        product.Name,
        product.Brand,
        product.Description,
        product.CategoryId,
        product.Condition,
        product.Price.Byn,
        product.Price.Rub,
        product.Availability,
        location,
        product.PresaleClosesAt,
        product.CanBeOrdered,
        product.Photos
            .OrderBy(p => p.SortOrder)
            .Select(p => storage.GetPublicUrl(p.StoragePath))
            .ToList(),
        product.Variants
            .Select(v => new ProductVariantDto(v.Id, v.Size, product.IsPresale || v.StockQuantity > 0))
            .ToList());

    public AdminProductListItemDto ToAdminListItem(Product product) => new(
        product.Id,
        product.Name,
        product.Brand,
        product.CategoryId,
        product.Status,
        product.Availability,
        product.Price.Byn,
        product.Price.Rub,
        product.Variants.Sum(v => v.StockQuantity),
        CoverUrl(product),
        product.UpdatedAt);

    public AdminProductDto ToAdmin(Product product) => new(
        product.Id,
        product.Name,
        product.Brand,
        product.Description,
        product.CategoryId,
        product.Condition,
        product.Price.Byn,
        product.Price.Rub,
        product.Availability,
        product.Status,
        product.PresaleLocationId,
        product.PresaleClosesAt,
        product.CreatedById,
        product.UpdatedById,
        product.CreatedAt,
        product.UpdatedAt,
        product.Photos
            .OrderBy(p => p.SortOrder)
            .Select(p => new AdminPhotoDto(p.Id, storage.GetPublicUrl(p.StoragePath), p.SortOrder))
            .ToList(),
        product.Variants
            .Select(v => new AdminVariantDto(v.Id, v.Size, v.StockQuantity))
            .ToList());

    public PresaleLocationDto ToLocationDto(PresaleLocation location) => new(
        location.Id,
        location.Name,
        location.Description,
        location.PhotoUrl is null ? null : storage.GetPublicUrl(location.PhotoUrl));
}