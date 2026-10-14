using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlyShop.Core.Entities;

namespace OnlyShop.Infrastructure.Persistence.Configurations;

public class BasketConfiguration : IEntityTypeConfiguration<Basket>
{
    public void Configure(EntityTypeBuilder<Basket> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.UserId).IsUnique();

        b.Metadata.FindNavigation(nameof(Basket.Items))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        b.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.BasketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class BasketItemConfiguration : IEntityTypeConfiguration<BasketItem>
{
    public void Configure(EntityTypeBuilder<BasketItem> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.BasketId, x.ProductVariantId }).IsUnique();
    }
}