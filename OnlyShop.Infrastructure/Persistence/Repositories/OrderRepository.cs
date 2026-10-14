using Microsoft.EntityFrameworkCore;
using OnlyShop.Application.Abstractions.Persistence;
using OnlyShop.Application.Common.Models;
using OnlyShop.Application.Models;
using OnlyShop.Core.Entities;

namespace OnlyShop.Infrastructure.Persistence.Repositories;

public class OrderRepository(OnlyShopDbContext db) : IOrderRepository
{
    public Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Orders.Include(x => x.Items).Include(x => x.StatusHistory)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<PagedResult<Order>> SearchAsync(OrderSearchCriteria c, CancellationToken ct = default)
    {
        var query = db.Orders.Include(x => x.Items).Include(x => x.StatusHistory).AsQueryable();

        if (c.Status is not null)
            query = query.Where(x => x.Status == c.Status);

        if (c.CustomerId is not null)
            query = query.Where(x => x.CustomerId == c.CustomerId);

        if (!string.IsNullOrWhiteSpace(c.Search))
            query = query.Where(x =>
                EF.Functions.ILike(x.CustomerName, $"%{c.Search}%")
                || (x.CustomerPhone != null && EF.Functions.ILike(x.CustomerPhone, $"%{c.Search}%")));

        if (c.From is not null)
            query = query.Where(x => x.CreatedAt >= c.From);

        if (c.To is not null)
            query = query.Where(x => x.CreatedAt <= c.To);

        query = query.OrderByDescending(x => x.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((c.Page - 1) * c.PageSize).Take(c.PageSize).ToListAsync(ct);

        return new PagedResult<Order>(items, c.Page, c.PageSize, total);
    }

    public void Add(Order order) => db.Orders.Add(order);
}