using Microsoft.EntityFrameworkCore;
using SmtOrderManager.Application.Contracts;
using SmtOrderManager.Application.Services;
using SmtOrderManager.Domain.Boards;
using SmtOrderManager.Domain.Common;
using SmtOrderManager.Infrastructure.Persistence;

namespace SmtOrderManager.Infrastructure.Services;

public sealed class EfBoardService : IBoardService
{
    private readonly AppDbContext _db;

    public EfBoardService(AppDbContext db) => _db = db;

    public async Task<BoardDto> CreateAsync(CreateBoardRequest request, CancellationToken ct)
    {
        var entity = new Board(request.Name, request.Description, request.Length, request.Width);
        _db.Boards.Add(entity);
        await _db.SaveChangesAsync(ct);

        return ToDto(entity);
    }

    public async Task<BoardDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var b = await _db.Boards
            .AsNoTracking()
            .Include(x => x.ComponentLinks)
            .SingleOrDefaultAsync(x => x.Id == id, ct);

        return b is null ? null : ToDto(b);
    }

    public async Task<IReadOnlyList<BoardDto>> SearchAsync(string? name, CancellationToken ct)
    {
        var q = _db.Boards.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(name))
            q = q.Where(x => EF.Functions.Like(x.Name, $"%{name.Trim()}%"));

        var list = await q
            .OrderBy(x => x.Name)
            .Include(x => x.ComponentLinks)
            .ToListAsync(ct);

        return list.Select(ToDto).ToList();
    }

    public async Task<BoardDto> UpdateAsync(Guid id, UpdateBoardRequest request, CancellationToken ct)
    {
        var entity = await _db.Boards
            .Include(x => x.ComponentLinks)
            .SingleOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null) throw new NotFoundException("Board not found.");

        entity.Update(request.Name, request.Description, request.Length, request.Width);
        await _db.SaveChangesAsync(ct);

        return ToDto(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var entity = await _db.Boards.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return false;

        _db.Boards.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<BoardDto> AddComponentAsync(Guid boardId, LinkComponentRequest request, CancellationToken ct)
    {
        var board = await _db.Boards
            .Include(x => x.ComponentLinks)
            .SingleOrDefaultAsync(x => x.Id == boardId, ct);

        if (board is null) throw new NotFoundException("Board not found.");

        var exists = await _db.Components.AnyAsync(x => x.Id == request.ComponentId, ct);
        if (!exists) throw new NotFoundException("Component not found.");

        board.AddComponent(request.ComponentId, request.PlacementQuantity);
        await _db.SaveChangesAsync(ct);

        return ToDto(board);
    }

    public async Task<BoardDto> RemoveComponentAsync(Guid boardId, Guid componentId, CancellationToken ct)
    {
        var board = await _db.Boards
            .Include(x => x.ComponentLinks)
            .SingleOrDefaultAsync(x => x.Id == boardId, ct);

        if (board is null) throw new NotFoundException("Board not found.");

        board.RemoveComponent(componentId);
        await _db.SaveChangesAsync(ct);

        return ToDto(board);
    }

    private static BoardDto ToDto(Board b) =>
        new(
            b.Id,
            b.Name,
            b.Description,
            b.Length,
            b.Width,
            b.ComponentLinks.Select(x => x.ComponentId).ToList()
        );
}
