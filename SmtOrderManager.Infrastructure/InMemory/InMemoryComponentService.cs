using SmtOrderManager.Application.Contracts;
using SmtOrderManager.Application.Services;
using SmtOrderManager.Domain.Common;
using SmtOrderManager.Domain.Components;

namespace SmtOrderManager.Infrastructure.InMemory;

public sealed class InMemoryComponentService : IComponentService
{
    private readonly InMemoryStore _store;

    public InMemoryComponentService(InMemoryStore store) => _store = store;

    public Task<ComponentDto> CreateAsync(CreateComponentRequest request, CancellationToken ct)
    {
        var entity = new Component(request.Name, request.Description, request.Quantity);
        _store.Components[entity.Id] = entity;
        return Task.FromResult(Mapping.ToDto(entity));
    }

    public Task<ComponentDto?> GetByIdAsync(Guid id, CancellationToken ct)
        => Task.FromResult(_store.Components.TryGetValue(id, out var c) ? Mapping.ToDto(c) : null);

    public Task<IReadOnlyList<ComponentDto>> SearchAsync(string? name, CancellationToken ct)
    {
        var query = _store.Components.Values.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(x => x.Name.Contains(name.Trim(), StringComparison.OrdinalIgnoreCase));

        return Task.FromResult<IReadOnlyList<ComponentDto>>(query.Select(Mapping.ToDto).ToList());
    }

    public Task<ComponentDto> UpdateAsync(Guid id, UpdateComponentRequest request, CancellationToken ct)
    {
        if (!_store.Components.TryGetValue(id, out var entity))
            throw new NotFoundException("Component not found.");

        entity.Update(request.Name, request.Description, request.Quantity);
        return Task.FromResult(Mapping.ToDto(entity));
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct)
        => Task.FromResult(_store.Components.TryRemove(id, out _));
}
