using OnlyShop.Core.Common;

namespace OnlyShop.Core.Entities;

public class ProductVariant : BaseEntity
{
    private ProductVariant()
    {
    }

    public Guid ProductId { get; private set; }
    public string Size { get; private set; } = null!;
    public int StockQuantity { get; private set; }

    public static ProductVariant Create(Guid productId, string size, int stockQuantity)
    {
        if (string.IsNullOrWhiteSpace(size))
        {
            throw new DomainException("Size is required");
        }

        if (stockQuantity < 0)
        {
            throw new DomainException("Stock cannot be negative");
        }

        return new ProductVariant
        {
            ProductId = productId,
            Size = size.Trim(),
            StockQuantity = stockQuantity
        };
    }

    public void Reserve(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("Quantity must be positive");
        }

        if (quantity > StockQuantity)
        {
            throw new DomainException($"Not enough stock for size {Size}");
        }

        StockQuantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Restock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("Quantity must be positive");
        }

        StockQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }
}