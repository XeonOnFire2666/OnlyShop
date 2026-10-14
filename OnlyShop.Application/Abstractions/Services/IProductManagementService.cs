using OnlyShop.Application.Common.Models;
using OnlyShop.Application.Dtos;

namespace OnlyShop.Application.Abstractions.Services;

public interface IProductManagementService
{
    Task<PagedResult<AdminProductListItemDto>> SearchAsync(AdminProductQuery query, CancellationToken ct = default);
    Task<AdminProductDto> GetAsync(Guid productId, CancellationToken ct = default);
    Task<AdminProductDto> CreateAsync(Guid staffId, CreateProductRequest request, CancellationToken ct = default);
    Task<AdminProductDto> UpdateDetailsAsync(Guid staffId, Guid productId, UpdateProductRequest request, CancellationToken ct = default);
    Task<AdminProductDto> ChangePriceAsync(Guid staffId, Guid productId, ChangePriceRequest request, CancellationToken ct = default);
    Task<AdminProductDto> SetPresaleAsync(Guid staffId, Guid productId, SetPresaleRequest request, CancellationToken ct = default);
    Task<AdminProductDto> SetInStockAsync(Guid staffId, Guid productId, CancellationToken ct = default);
    Task<AdminProductDto> AddVariantAsync(Guid staffId, Guid productId, VariantInput request, CancellationToken ct = default);
    Task<AdminProductDto> AdjustStockAsync(Guid staffId, Guid productId, Guid variantId, int delta, CancellationToken ct = default);
    Task<AdminProductDto> AddPhotoAsync(Guid staffId, Guid productId, FileUpload file, CancellationToken ct = default);
    Task<AdminProductDto> PublishAsync(Guid staffId, Guid productId, CancellationToken ct = default);
    Task<AdminProductDto> ArchiveAsync(Guid staffId, Guid productId, CancellationToken ct = default);
}