using SmtOrderManager.Application.Contracts;

namespace SmtOrderManager.Application.Services;

public interface IComponentService
{
    Task<ComponentDto> CreateAsync(CreateComponentRequest request, CancellationToken ct);
    Task<ComponentDto?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<ComponentDto>> SearchAsync(string? name, CancellationToken ct);
    Task<ComponentDto> UpdateAsync(Guid id, UpdateComponentRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}
