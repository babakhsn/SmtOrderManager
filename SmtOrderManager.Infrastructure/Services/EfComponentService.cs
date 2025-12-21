using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Application.Contracts;
using SmtOrderManager.Application.Services;
using SmtOrderManager.Domain.Common;
using SmtOrderManager.Domain.Components;
using SmtOrderManager.Infrastructure.Persistence;

namespace SmtOrderManager.Infrastructure.Services;

public sealed class EfComponentService : IComponentService
{
    private readonly AppDbContext _db;
    private readonly ILogger<EfComponentService> _logger;

    public EfComponentService(AppDbContext db, ILogger<EfComponentService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ComponentDto> CreateAsync(CreateComponentRequest request, CancellationToken ct)
    {
        var entity = new Component(request.Name, request.Description, request.Quantity);
        _db.Components.Add(entity);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Component created. ComponentId={ComponentId} Name={Name}", entity.Id, entity.Name);

        return new ComponentDto(entity.Id, entity.Name, entity.Description, entity.Quantity);
    }

    public async Task<ComponentDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var c = await _db.Components.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        return c is null ? null : new ComponentDto(c.Id, c.Name, c.Description, c.Quantity);
    }

    public async Task<IReadOnlyList<ComponentDto>> SearchAsync(string? name, CancellationToken ct)
    {
        var q = _db.Components.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(name))
            q = q.Where(x => EF.Functions.Like(x.Name, $"%{name.Trim()}%"));

        var list = await q.OrderBy(x => x.Name).ToListAsync(ct);
        return list.Select(c => new ComponentDto(c.Id, c.Name, c.Description, c.Quantity)).ToList();
    }

    public async Task<ComponentDto> UpdateAsync(Guid id, UpdateComponentRequest request, CancellationToken ct)
    {
        var entity = await _db.Components.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) throw new NotFoundException("Component not found.");

        entity.Update(request.Name, request.Description, request.Quantity);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Component updated. ComponentId={ComponentId}", entity.Id);

        return new ComponentDto(entity.Id, entity.Name, entity.Description, entity.Quantity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var entity = await _db.Components.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return false;

        _db.Components.Remove(entity);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Component deleted. ComponentId={ComponentId}", id);


        return true;
    }
}
