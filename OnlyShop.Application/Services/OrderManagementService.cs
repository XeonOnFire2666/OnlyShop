using OnlyShop.Application.Abstractions.Persistence;
using OnlyShop.Application.Abstractions.Services;
using OnlyShop.Application.Common;
using OnlyShop.Application.Common.Exceptions;
using OnlyShop.Application.Common.Models;
using OnlyShop.Application.Dtos;
using OnlyShop.Application.Mapping;
using OnlyShop.Application.Models;
using OnlyShop.Core.Entities;

namespace OnlyShop.Application.Services;

internal sealed class OrderManagementService(
    IOrderRepository orders,
    IUserRepository users,
    OrderWorkflow workflow) : IOrderManagementService
{
    public async Task<PagedResult<OrderDto>> SearchAsync(AdminOrderQuery query, CancellationToken ct = default)
    {
        var (page, pageSize) = PagingRules.Normalize(query.Page, query.PageSize, 100);

        var criteria = new OrderSearchCriteria(
            query.Status,
            null,
            query.Search?.Trim(),
            query.From,
            query.To,
            page,
            pageSize);

        var result = await orders.SearchAsync(criteria, ct);
        return result.Map(o => o.ToDto());
    }

    public async Task<OrderDetailsDto> GetAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await LoadAsync(orderId, ct);

        var customer = await users.GetByIdAsync(order.CustomerId, ct)
            ?? throw new NotFoundException(nameof(User), order.CustomerId);

        return new OrderDetailsDto(order.ToDto(), customer.ToDto());
    }

    public async Task<OrderDto> ChangeStatusAsync(Guid? staffId, Guid orderId, ChangeOrderStatusRequest request, CancellationToken ct = default)
    {
        var order = await LoadAsync(orderId, ct);
        await workflow.ChangeStatusAsync(order, request.Status, staffId, request.Note, ct);
        return order.ToDto();
    }

    private async Task<Order> LoadAsync(Guid orderId, CancellationToken ct) =>
        await orders.GetByIdAsync(orderId, ct)
            ?? throw new NotFoundException(nameof(Order), orderId);
}