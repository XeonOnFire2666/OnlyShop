using Microsoft.EntityFrameworkCore;
using OnlyShop.Application.Abstractions.Persistence;
using OnlyShop.Core.Entities;
using System.Reflection.Emit;

namespace OnlyShop.Infrastructure.Persistence;

public class OnlyShopDbContext(DbContextOptions<OnlyShopDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users => Set<User>();
    public DbSet<StaffMember> StaffMembers => Set<StaffMember>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<PresaleLocation> PresaleLocations => Set<PresaleLocation>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Basket> Baskets => Set<Basket>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(OnlyShopDbContext).Assembly);
        base.OnModelCreating(builder);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => base.SaveChangesAsync(ct);
}