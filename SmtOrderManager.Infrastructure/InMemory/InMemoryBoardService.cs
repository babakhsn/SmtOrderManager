using SmtOrderManager.Application.Contracts;
using SmtOrderManager.Application.Services;
using SmtOrderManager.Domain.Boards;

namespace SmtOrderManager.Infrastructure.InMemory;

public sealed class InMemoryBoardService : IBoardService
{
    private readonly InMemoryStore _store;

    public InMemoryBoardService(InMemoryStore store) => _store = store;

    public Task<BoardDto> CreateAsync(CreateBoardRequest request, CancellationToken ct)
    {
        var entity = new Board(request.Name, request.Description, request.Length, request.Width);
        _store.Boards[entity.Id] = entity;
        return Task.FromResult(Mapping.ToDto(entity));
    }

    public Task<BoardDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return Task.FromResult(_store.Boards.TryGetValue(id, out var b) ? Mapping.ToDto(b) : null);
    }

    public Task<IReadOnlyList<BoardDto>> SearchAsync(string? name, CancellationToken ct)
    {
        var query = _store.Boards.Values.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(x => x.Name.Contains(name.Trim(), StringComparison.OrdinalIgnoreCase));

        return Task.FromResult<IReadOnlyList<BoardDto>>(query.Select(Mapping.ToDto).ToList());
    }

    public Task<BoardDto> UpdateAsync(Guid id, UpdateBoardRequest request, CancellationToken ct)
    {
        if (!_store.Boards.TryGetValue(id, out var entity))
            throw new InvalidOperationException("Board not found.");

        entity.Update(request.Name, request.Description, request.Length, request.Width);
        return Task.FromResult(Mapping.ToDto(entity));
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        // Optional: also unlink from orders; leaving as-is for Phase 1 skeleton
        return Task.FromResult(_store.Boards.TryRemove(id, out _));
    }

    public Task<BoardDto> AddComponentAsync(Guid boardId, LinkComponentRequest request, CancellationToken ct)
    {
        if (!_store.Boards.TryGetValue(boardId, out var board))
            throw new InvalidOperationException("Board not found.");

        if (!_store.Components.ContainsKey(request.ComponentId))
            throw new InvalidOperationException("Component not found.");

        board.AddComponent(request.ComponentId, request.PlacementQuantity);
        return Task.FromResult(Mapping.ToDto(board));
    }

    public Task<BoardDto> RemoveComponentAsync(Guid boardId, Guid componentId, CancellationToken ct)
    {
        if (!_store.Boards.TryGetValue(boardId, out var board))
            throw new InvalidOperationException("Board not found.");

        board.RemoveComponent(componentId);
        return Task.FromResult(Mapping.ToDto(board));
    }
}
