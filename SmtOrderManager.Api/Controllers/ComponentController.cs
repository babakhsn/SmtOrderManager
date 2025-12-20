using Microsoft.AspNetCore.Mvc;
using SmtOrderManager.Application.Contracts;
using SmtOrderManager.Application.Services;

namespace SmtOrderManager.Api.Controllers;

[ApiController]
[Route("api/components")]
public sealed class ComponentsController : ControllerBase
{
    private readonly IComponentService _service;

    public ComponentsController(IComponentService service) => _service = service;

    [HttpPost]
    public async Task<ActionResult<ComponentDto>> Create([FromBody] CreateComponentRequest request, CancellationToken ct)
        => Ok(await _service.CreateAsync(request, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ComponentDto>> GetById(Guid id, CancellationToken ct)
    {
        var dto = await _service.GetByIdAsync(id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ComponentDto>>> Search([FromQuery] string? name, CancellationToken ct)
        => Ok(await _service.SearchAsync(name, ct));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ComponentDto>> Update(Guid id, [FromBody] UpdateComponentRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, request, ct));

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
        => (await _service.DeleteAsync(id, ct)) ? NoContent() : NotFound();
}
