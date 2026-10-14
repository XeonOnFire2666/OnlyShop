using OnlyShop.Core.Common;

namespace OnlyShop.Core.Entities;

public class ProductPhoto : BaseEntity
{
    private ProductPhoto()
    {
    }

    public Guid ProductId { get; private set; }
    public string StoragePath { get; private set; } = null!;
    public string? TelegramFileId { get; private set; }
    public int SortOrder { get; private set; }

    public static ProductPhoto Create(Guid productId, string storagePath, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(storagePath))
        {
            throw new DomainException("Photo path is required");
        }

        return new ProductPhoto
        {
            ProductId = productId,
            StoragePath = storagePath,
            SortOrder = sortOrder
        };
    }

    public void CacheTelegramFileId(string fileId) => TelegramFileId = fileId;
}