using OnlyShop.Application.Common.Models;
using OnlyShop.Application.Dtos;

namespace OnlyShop.Application.Abstractions.Services;

public interface IOrderManagementService
{
    Task<PagedResult<OrderDto>> SearchAsync(AdminOrderQuery query, CancellationToken ct = default);
    Task<OrderDetailsDto> GetAsync(Guid orderId, CancellationToken ct = default);
    Task<OrderDto> ChangeStatusAsync(Guid staffId, Guid orderId, ChangeOrderStatusRequest request, CancellationToken ct = default);
}