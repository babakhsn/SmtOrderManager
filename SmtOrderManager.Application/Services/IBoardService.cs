using SmtOrderManager.Application.Contracts;

namespace SmtOrderManager.Application.Services;

public interface IBoardService
{
    Task<BoardDto> CreateAsync(CreateBoardRequest request, CancellationToken ct);
    Task<BoardDto?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<BoardDto>> SearchAsync(string? name, CancellationToken ct);
    Task<BoardDto> UpdateAsync(Guid id, UpdateBoardRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);

    Task<BoardDto> AddComponentAsync(Guid boardId, LinkComponentRequest request, CancellationToken ct);
    Task<BoardDto> RemoveComponentAsync(Guid boardId, Guid componentId, CancellationToken ct);
}
