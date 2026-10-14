using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlyShop.Core.Entities;

namespace OnlyShop.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.CustomerName).HasMaxLength(200).IsRequired();
        b.Property(x => x.CustomerPhone).HasMaxLength(50);
        b.Property(x => x.BonusesSpent).HasColumnType("decimal(18,2)");

        b.HasIndex(x => x.CustomerId);
        b.HasIndex(x => x.Status);

        b.Metadata.FindNavigation(nameof(Order.Items))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
        b.Metadata.FindNavigation(nameof(Order.StatusHistory))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        b.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(x => x.StatusHistory)
            .WithOne()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.ProductName).HasMaxLength(300).IsRequired();
        b.Property(x => x.Size).HasMaxLength(50).IsRequired();
        b.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
        b.Ignore(x => x.Total);
    }
}

public class OrderStatusChangeConfiguration : IEntityTypeConfiguration<OrderStatusChange>
{
    public void Configure(EntityTypeBuilder<OrderStatusChange> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Note).HasMaxLength(500);
    }
}