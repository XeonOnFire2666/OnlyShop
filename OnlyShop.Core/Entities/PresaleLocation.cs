using OnlyShop.Core.Common;

namespace OnlyShop.Core.Entities;

public class PresaleLocation : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? PhotoUrl { get; set; }

    public List<Product> Products { get; init; } = [];
}