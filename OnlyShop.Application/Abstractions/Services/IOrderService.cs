using OnlyShop.Application.Common.Models;
using OnlyShop.Application.Dtos;

namespace OnlyShop.Application.Abstractions.Services;

public interface IOrderService
{
    Task<OrderDto> CheckoutAsync(Guid userId, CheckoutRequest request, CancellationToken ct = default);
    Task<PagedResult<OrderDto>> GetMyOrdersAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task<OrderDto> GetMyOrderAsync(Guid userId, Guid orderId, CancellationToken ct = default);
    Task<OrderDto> CancelMyOrderAsync(Guid userId, Guid orderId, CancellationToken ct = default);
}