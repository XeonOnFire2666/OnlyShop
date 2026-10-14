using OnlyShop.Core.Common;
using OnlyShop.Core.Enums;
using OnlyShop.Core.ValueObjects;

namespace OnlyShop.Core.Entities;

public class Product : BaseEntity
{
    private readonly List<ProductVariant> _variants = [];
    private readonly List<ProductPhoto> _photos = [];

    private Product()
    {
    }

    public string Name { get; private set; } = null!;
    public string? Brand { get; private set; }
    public string? Description { get; private set; }
    public Guid CategoryId { get; private set; }
    public ProductCondition Condition { get; private set; }
    public ProductPrice Price { get; private set; }
    public ProductAvailability Availability { get; private set; }
    public ProductStatus Status { get; private set; }
    public Guid? PresaleLocationId { get; private set; }
    public DateTime? PresaleClosesAt { get; private set; }
    public Guid CreatedById { get; private set; }
    public Guid UpdatedById { get; private set; }

    public IReadOnlyList<ProductVariant> Variants => _variants;
    public IReadOnlyList<ProductPhoto> Photos => _photos;

    public bool IsPublished => Status == ProductStatus.Published;
    public bool IsPresale => Availability == ProductAvailability.Presale;
    public bool IsPresaleClosed => IsPresale && PresaleClosesAt <= DateTime.UtcNow;
    public bool HasStock => _variants.Any(v => v.StockQuantity > 0);
    public bool CanBeOrdered => IsPublished && (IsPresale ? !IsPresaleClosed : HasStock);

    public static Product Create(
        string name,
        Guid categoryId,
        ProductPrice price,
        ProductCondition condition,
        Guid staffId,
        string? brand = null,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Product name is required");
        }

        return new Product
        {
            Name = name.Trim(),
            CategoryId = categoryId,
            Price = price,
            Condition = condition,
            Brand = brand,
            Description = description,
            Availability = ProductAvailability.InStock,
            Status = ProductStatus.Draft,
            CreatedById = staffId,
            UpdatedById = staffId
        };
    }

    public void UpdateDetails(string name, string? brand, string? description, ProductCondition condition, Guid staffId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Product name is required");
        }

        Name = name.Trim();
        Brand = brand;
        Description = description;
        Condition = condition;
        Touch(staffId);
    }

    public void ChangePrice(ProductPrice price, Guid staffId)
    {
        Price = price;
        Touch(staffId);
    }

    public void MarkAsInStock(Guid staffId)
    {
        Availability = ProductAvailability.InStock;
        PresaleLocationId = null;
        PresaleClosesAt = null;
        Touch(staffId);
    }

    public void MarkAsPresale(Guid locationId, DateTime closesAt, Guid staffId)
    {
        if (closesAt <= DateTime.UtcNow)
        {
            throw new DomainException("Presale close date must be in the future");
        }

        Availability = ProductAvailability.Presale;
        PresaleLocationId = locationId;
        PresaleClosesAt = closesAt;
        Touch(staffId);
    }

    public ProductVariant AddVariant(string size, int stockQuantity, Guid staffId)
    {
        if (_variants.Any(v => string.Equals(v.Size, size.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainException($"Size {size} already exists");
        }

        var variant = ProductVariant.Create(Id, size, stockQuantity);
        _variants.Add(variant);
        Touch(staffId);
        return variant;
    }

    public ProductPhoto AddPhoto(string storagePath, Guid staffId)
    {
        var photo = ProductPhoto.Create(Id, storagePath, _photos.Count);
        _photos.Add(photo);
        Touch(staffId);
        return photo;
    }

    public void Publish(Guid staffId)
    {
        if (_photos.Count == 0)
        {
            throw new DomainException("Cannot publish a product without photos");
        }

        if (_variants.Count == 0)
        {
            throw new DomainException("Cannot publish a product without sizes");
        }

        if (IsPresale && IsPresaleClosed)
        {
            throw new DomainException("Cannot publish a presale that is already closed");
        }

        Status = ProductStatus.Published;
        Touch(staffId);
    }

    public void Archive(Guid staffId)
    {
        Status = ProductStatus.Archived;
        Touch(staffId);
    }

    public void ReserveStock(Guid variantId, int quantity)
    {
        if (!CanBeOrdered)
        {
            throw new DomainException("Product is not available for ordering");
        }

        var variant = _variants.FirstOrDefault(v => v.Id == variantId)
            ?? throw new DomainException("Size not found");

        if (IsPresale)
        {
            if (quantity <= 0)
            {
                throw new DomainException("Quantity must be positive");
            }

            return;
        }

        variant.Reserve(quantity);
    }

    public void ReleaseStock(Guid variantId, int quantity)
    {
        if (IsPresale)
        {
            return;
        }

        var variant = _variants.FirstOrDefault(v => v.Id == variantId)
            ?? throw new DomainException("Size not found");

        variant.Restock(quantity);
    }

    private void Touch(Guid staffId)
    {
        UpdatedById = staffId;
        UpdatedAt = DateTime.UtcNow;
    }
}