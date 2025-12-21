using System.Text;
using Microsoft.EntityFrameworkCore;
using SmtOrderManager.Application.Abstractions;
using SmtOrderManager.Application.Contracts;
using SmtOrderManager.Application.Services;
using SmtOrderManager.Domain.Common;
using SmtOrderManager.Domain.Orders;
using SmtOrderManager.Infrastructure.Persistence;

namespace SmtOrderManager.Infrastructure.Services;

public sealed class EfOrderService : IOrderService
{
    private readonly AppDbContext _db;
    private readonly ITimeProvider _time;
    private readonly IJsonSerializer _json;

    public EfOrderService(AppDbContext db, ITimeProvider time, IJsonSerializer json)
    {
        _db = db;
        _time = time;
        _json = json;
    }

    public async Task<OrderDto> CreateAsync(CreateOrderRequest request, CancellationToken ct)
    {
        var entity = new Order(request.Name, request.Description, request.OrderDate);
        _db.Orders.Add(entity);
        await _db.SaveChangesAsync(ct);
        return ToDto(entity);
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var o = await _db.Orders
            .AsNoTracking()
            .Include(x => x.BoardLinks)
            .SingleOrDefaultAsync(x => x.Id == id, ct);

        return o is null ? null : ToDto(o);
    }

    public async Task<IReadOnlyList<OrderDto>> SearchAsync(string? name, CancellationToken ct)
    {
        var q = _db.Orders.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(name))
            q = q.Where(x => EF.Functions.Like(x.Name, $"%{name.Trim()}%"));

        var list = await q
            .Include(x => x.BoardLinks)
            .ToListAsync(ct);

        return list
            .OrderByDescending(x => x.OrderDate) // LINQ to Objects
            .Select(ToDto)
            .ToList();

    }

    public async Task<OrderDto> UpdateAsync(Guid id, UpdateOrderRequest request, CancellationToken ct)
    {
        var entity = await _db.Orders
            .Include(x => x.BoardLinks)
            .SingleOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null) throw new NotFoundException("Order not found.");

        entity.Update(request.Name, request.Description, request.OrderDate);
        await _db.SaveChangesAsync(ct);

        return ToDto(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var entity = await _db.Orders.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return false;

        _db.Orders.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<OrderDto> AddBoardAsync(Guid orderId, Guid boardId, CancellationToken ct)
    {
        var order = await _db.Orders
            .Include(x => x.BoardLinks)
            .SingleOrDefaultAsync(x => x.Id == orderId, ct);

        if (order is null) throw new NotFoundException("Order not found.");

        var boardExists = await _db.Boards.AnyAsync(x => x.Id == boardId, ct);
        if (!boardExists) throw new NotFoundException("Board not found.");

        order.AddBoard(boardId);
        await _db.SaveChangesAsync(ct);

        return ToDto(order);
    }

    public async Task<OrderDto> RemoveBoardAsync(Guid orderId, Guid boardId, CancellationToken ct)
    {
        var order = await _db.Orders
            .Include(x => x.BoardLinks)
            .SingleOrDefaultAsync(x => x.Id == orderId, ct);

        if (order is null) throw new NotFoundException("Order not found.");

        order.RemoveBoard(boardId);
        await _db.SaveChangesAsync(ct);

        return ToDto(order);
    }

    public async Task<DownloadPayload> DownloadAsync(Guid orderId, CancellationToken ct)
    {
        var order = await _db.Orders
            .AsNoTracking()
            .Include(x => x.BoardLinks)
            .SingleOrDefaultAsync(x => x.Id == orderId, ct);

        if (order is null) throw new NotFoundException("Order not found.");

        var model = new
        {
            DownloadedAtUtc = _time.UtcNow,
            Order = ToDto(order)
        };

        var json = _json.Serialize(model);
        var content = Encoding.UTF8.GetBytes(json);
        var fileName = $"order_{order.Id}_{_time.UtcNow:yyyyMMdd_HHmmss}_utc.json";

        return new DownloadPayload(fileName, "application/json", content);
    }

    private static OrderDto ToDto(Order o) =>
        new(
            o.Id,
            o.Name,
            o.Description,
            o.OrderDate,
            o.BoardLinks.Select(x => x.BoardId).ToList()
        );
}
