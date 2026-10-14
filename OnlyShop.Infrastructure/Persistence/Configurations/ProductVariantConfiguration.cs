using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlyShop.Core.Entities;

namespace OnlyShop.Infrastructure.Persistence.Configurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Size).HasMaxLength(50).IsRequired();
    }
}