using System.Text;
using SmtOrderManager.Application.Abstractions;
using SmtOrderManager.Application.Contracts;
using SmtOrderManager.Application.Services;
using SmtOrderManager.Domain.Common;
using SmtOrderManager.Domain.Orders;

namespace SmtOrderManager.Infrastructure.InMemory;

public sealed class InMemoryOrderService : IOrderService
{
    private readonly InMemoryStore _store;
    private readonly ITimeProvider _time;
    private readonly IJsonSerializer _json;

    public InMemoryOrderService(InMemoryStore store, ITimeProvider time, IJsonSerializer json)
    {
        _store = store;
        _time = time;
        _json = json;
    }

    public Task<OrderDto> CreateAsync(CreateOrderRequest request, CancellationToken ct)
    {
        var entity = new Order(request.Name, request.Description, request.OrderDate);
        _store.Orders[entity.Id] = entity;
        return Task.FromResult(Mapping.ToDto(entity));
    }

    public Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken ct)
        => Task.FromResult(_store.Orders.TryGetValue(id, out var o) ? Mapping.ToDto(o) : null);

    public Task<IReadOnlyList<OrderDto>> SearchAsync(string? name, CancellationToken ct)
    {
        var query = _store.Orders.Values.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(x => x.Name.Contains(name.Trim(), StringComparison.OrdinalIgnoreCase));

        return Task.FromResult<IReadOnlyList<OrderDto>>(query.Select(Mapping.ToDto).ToList());
    }

    public Task<OrderDto> UpdateAsync(Guid id, UpdateOrderRequest request, CancellationToken ct)
    {
        if (!_store.Orders.TryGetValue(id, out var entity))
            throw new NotFoundException("Order not found.");

        entity.Update(request.Name, request.Description, request.OrderDate);
        return Task.FromResult(Mapping.ToDto(entity));
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct)
        => Task.FromResult(_store.Orders.TryRemove(id, out _));

    public Task<OrderDto> AddBoardAsync(Guid orderId, Guid boardId, CancellationToken ct)
    {
        if (!_store.Orders.TryGetValue(orderId, out var order))
            throw new NotFoundException("Order not found.");

        if (!_store.Boards.ContainsKey(boardId))
            throw new NotFoundException("Board not found.");

        order.AddBoard(boardId);
        return Task.FromResult(Mapping.ToDto(order));
    }

    public Task<OrderDto> RemoveBoardAsync(Guid orderId, Guid boardId, CancellationToken ct)
    {
        if (!_store.Orders.TryGetValue(orderId, out var order))
            throw new NotFoundException("Order not found.");

        order.RemoveBoard(boardId);
        return Task.FromResult(Mapping.ToDto(order));
    }

    public Task<DownloadPayload> DownloadAsync(Guid orderId, CancellationToken ct)
    {
        if (!_store.Orders.TryGetValue(orderId, out var order))
            throw new NotFoundException("Order not found.");

        var model = new
        {
            DownloadedAtUtc = _time.UtcNow,
            Order = Mapping.ToDto(order)
        };

        var json = _json.Serialize(model);
        var content = Encoding.UTF8.GetBytes(json);
        var fileName = $"order_{order.Id}_{_time.UtcNow:yyyyMMdd_HHmmss}_utc.json";

        return Task.FromResult(new DownloadPayload(fileName, "application/json", content));
    }
}
