using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<EfOrderService> _logger;

    public EfOrderService(AppDbContext db, ITimeProvider time, IJsonSerializer json, ILogger<EfOrderService> logger)
    {
        _db = db;
        _time = time;
        _json = json;
        _logger = logger;
    }

    public async Task<OrderDto> CreateAsync(CreateOrderRequest request, CancellationToken ct)
    {
        var entity = new Order(request.Name, request.Description, request.OrderDate);
        _db.Orders.Add(entity);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Order created. OrderId={OrderId} Name={Name} OrderDate={OrderDate}",
            entity.Id, entity.Name, entity.OrderDate);

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

        // SQLite limitation: DateTimeOffset cannot be used in ORDER BY server-side.
        var list = await q
            .Include(x => x.BoardLinks)
            .ToListAsync(ct);

        return list
            .OrderByDescending(x => x.OrderDate)
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

        _logger.LogInformation("Order updated. OrderId={OrderId}", entity.Id);

        return ToDto(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var entity = await _db.Orders.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return false;

        _db.Orders.Remove(entity);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Order deleted. OrderId={OrderId}", id);

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

        _logger.LogInformation("Board linked to order. OrderId={OrderId} BoardId={BoardId}", orderId, boardId);

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

        _logger.LogInformation("Board unlinked from order. OrderId={OrderId} BoardId={BoardId}", orderId, boardId);

        return ToDto(order);
    }

    public async Task<DownloadPayload> DownloadAsync(Guid orderId, CancellationToken ct)
    {
        // Load order + linked boards
        var order = await _db.Orders
            .AsNoTracking()
            .Include(x => x.BoardLinks)
            .SingleOrDefaultAsync(x => x.Id == orderId, ct);

        if (order is null) throw new NotFoundException("Order not found.");

        var boardIds = order.BoardLinks.Select(x => x.BoardId).Distinct().ToList();

        // Load boards + their component placements
        var boards = await _db.Boards
            .AsNoTracking()
            .Where(b => boardIds.Contains(b.Id))
            .Include(b => b.ComponentLinks)
            .ToListAsync(ct);

        // Load all component ids referenced by placements
        var componentIds = boards
            .SelectMany(b => b.ComponentLinks.Select(p => p.ComponentId))
            .Distinct()
            .ToList();

        var components = await _db.Components
            .AsNoTracking()
            .Where(c => componentIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, ct);

        var downloadedAt = _time.UtcNow;

        // Build download DTOs
        var orderDto = new OrderDownloadDto(order.Id, order.Name, order.Description, order.OrderDate);

        var boardDtos = boards.Select(b =>
        {
            var placements = b.ComponentLinks.Select(p =>
            {
                components.TryGetValue(p.ComponentId, out var comp);

                // If a component was deleted after linkage, keep the ID but mark unknown.
                var name = comp?.Name ?? "UNKNOWN";
                var desc = comp?.Description ?? string.Empty;

                return new BoardPlacementDto(
                    p.ComponentId,
                    name,
                    desc,
                    p.PlacementQuantity
                );
            }).ToList();

            return new BoardDownloadDto(
                b.Id,
                b.Name,
                b.Description,
                b.Length,
                b.Width,
                placements
            );
        }).ToList();

        // Compute BOM totals across all boards (sum placement quantities)
        var bom = boardDtos
            .SelectMany(b => b.Placements)
            .GroupBy(p => new { p.ComponentId, p.ComponentName })
            .Select(g => new BomLineDto(
                g.Key.ComponentId,
                g.Key.ComponentName,
                g.Sum(x => x.PlacementQuantity)
            ))
            .OrderBy(x => x.ComponentName)
            .ToList();

        var downloadModel = new ProductionOrderDownloadDto(
            downloadedAt,
            orderDto,
            boardDtos,
            bom
        );

        var json = _json.Serialize(downloadModel);
        var content = Encoding.UTF8.GetBytes(json);
        var fileName = $"order_{order.Id}_{downloadedAt:yyyyMMdd_HHmmss}_utc.json";

        _logger.LogInformation("Order downloaded (production payload). OrderId={OrderId} Boards={BoardCount} BomLines={BomLines} FileName={FileName}",
            orderId, boardDtos.Count, bom.Count, fileName);

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
