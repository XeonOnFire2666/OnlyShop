using OnlyShop.Application.Abstractions.Infrastructure;
using OnlyShop.Application.Abstractions.Persistence;
using OnlyShop.Application.Abstractions.Services;
using OnlyShop.Application.Common.Exceptions;
using OnlyShop.Application.Common.Models;
using OnlyShop.Application.Dtos;
using OnlyShop.Application.Mapping;
using OnlyShop.Core.Common;
using OnlyShop.Core.Entities;

namespace OnlyShop.Application.Services;

internal sealed class PresaleLocationService(
    IPresaleLocationRepository locations,
    IProductRepository products,
    IFileStorage storage,
    ProductMapper mapper,
    IUnitOfWork uow) : IPresaleLocationService
{
    public async Task<IReadOnlyList<PresaleLocationDto>> GetAllAsync(CancellationToken ct = default)
    {
        var all = await locations.GetAllAsync(ct);
        return all.OrderBy(l => l.Name).Select(mapper.ToLocationDto).ToList();
    }

    public async Task<PresaleLocationDto> CreateAsync(PresaleLocationRequest request, FileUpload? photo, CancellationToken ct = default)
    {
        var name = RequireName(request);

        var location = new PresaleLocation
        {
            Name = name,
            Description = request.Description?.Trim()
        };

        if (photo is not null)
        {
            location.PhotoUrl = await SavePhotoAsync(photo, ct);
        }

        locations.Add(location);
        await uow.SaveChangesAsync(ct);

        return mapper.ToLocationDto(location);
    }

    public async Task<PresaleLocationDto> UpdateAsync(Guid locationId, PresaleLocationRequest request, FileUpload? photo, CancellationToken ct = default)
    {
        var location = await locations.GetByIdAsync(locationId, ct)
            ?? throw new NotFoundException(nameof(PresaleLocation), locationId);

        location.Name = RequireName(request);
        location.Description = request.Description?.Trim();
        location.UpdatedAt = DateTime.UtcNow;

        string? oldPhoto = null;

        if (photo is not null)
        {
            oldPhoto = location.PhotoUrl;
            location.PhotoUrl = await SavePhotoAsync(photo, ct);
        }

        await uow.SaveChangesAsync(ct);

        if (oldPhoto is not null)
        {
            await storage.DeleteAsync(oldPhoto, ct);
        }

        return mapper.ToLocationDto(location);
    }

    public async Task DeleteAsync(Guid locationId, CancellationToken ct = default)
    {
        var location = await locations.GetByIdAsync(locationId, ct)
            ?? throw new NotFoundException(nameof(PresaleLocation), locationId);

        if (await products.AnyForPresaleLocationAsync(locationId, ct))
        {
            throw new DomainException("Cannot delete a location that is used by products");
        }

        var photo = location.PhotoUrl;

        locations.Remove(location);
        await uow.SaveChangesAsync(ct);

        if (photo is not null)
        {
            await storage.DeleteAsync(photo, ct);
        }
    }

    private static string RequireName(PresaleLocationRequest request)
    {
        var name = request.Name?.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Location name is required");
        }

        return name;
    }

    private async Task<string> SavePhotoAsync(FileUpload photo, CancellationToken ct)
    {
        if (!photo.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException("Only image files are allowed");
        }

        return await storage.SaveAsync(photo, "locations", ct);
    }
}