using SmtOrderManager.Application.Contracts;

namespace SmtOrderManager.Application.Services;

public interface IOrderService
{
    Task<OrderDto> CreateAsync(CreateOrderRequest request, CancellationToken ct);
    Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<OrderDto>> SearchAsync(string? name, CancellationToken ct);
    Task<OrderDto> UpdateAsync(Guid id, UpdateOrderRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);

    Task<OrderDto> AddBoardAsync(Guid orderId, Guid boardId, CancellationToken ct);
    Task<OrderDto> RemoveBoardAsync(Guid orderId, Guid boardId, CancellationToken ct);

    Task<DownloadPayload> DownloadAsync(Guid orderId, CancellationToken ct);
}
