using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlyShop.Core.Entities;

namespace OnlyShop.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(300).IsRequired();
        b.Property(x => x.Brand).HasMaxLength(200);

        // ProductPrice value object -> columns
        b.ComplexProperty(x => x.Price, p =>
        {
            p.Property(x => x.Byn).HasColumnName("PriceByn").HasColumnType("decimal(18,2)");
            p.Property(x => x.Rub).HasColumnName("PriceRub").HasColumnType("decimal(18,2)");
        });

        b.HasIndex(x => x.CategoryId);
        b.HasIndex(x => x.Status);

        b.Metadata.FindNavigation(nameof(Product.Variants))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
        b.Metadata.FindNavigation(nameof(Product.Photos))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        b.HasMany(x => x.Variants)
            .WithOne()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(x => x.Photos)
            .WithOne()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}