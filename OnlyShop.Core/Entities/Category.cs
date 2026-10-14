using OnlyShop.Core.Common;

namespace OnlyShop.Core.Entities;

public class Category : BaseEntity
{
    private Category()
    {
    }

    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public Guid? ParentId { get; private set; }
    public int SortOrder { get; private set; }

    public static Category Create(string name, string slug, Guid? parentId, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Category name is required");
        }

        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new DomainException("Category slug is required");
        }

        return new Category
        {
            Name = name.Trim(),
            Slug = slug.Trim().ToLowerInvariant(),
            ParentId = parentId,
            SortOrder = sortOrder
        };
    }

    public void Update(string name, string slug, Guid? parentId, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Category name is required");
        }

        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new DomainException("Category slug is required");
        }

        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        ParentId = parentId;
        SortOrder = sortOrder;
        UpdatedAt = DateTime.UtcNow;
    }
}