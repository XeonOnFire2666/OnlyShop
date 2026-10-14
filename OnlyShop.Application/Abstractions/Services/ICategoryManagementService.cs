using OnlyShop.Application.Dtos;

namespace OnlyShop.Application.Abstractions.Services;

public interface ICategoryManagementService
{
    Task<CategoryDto> CreateAsync(CategoryRequest request, CancellationToken ct = default);
    Task<CategoryDto> UpdateAsync(Guid categoryId, CategoryRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid categoryId, CancellationToken ct = default);
}