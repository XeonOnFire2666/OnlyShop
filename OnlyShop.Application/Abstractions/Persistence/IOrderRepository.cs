using OnlyShop.Application.Common.Models;
using OnlyShop.Application.Models;
using OnlyShop.Core.Entities;

namespace OnlyShop.Application.Abstractions.Persistence;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<Order>> SearchAsync(OrderSearchCriteria criteria, CancellationToken ct = default);
    void Add(Order order);
}