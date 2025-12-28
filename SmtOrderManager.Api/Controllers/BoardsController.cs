using Microsoft.AspNetCore.Mvc;
using SmtOrderManager.Application.Contracts;
using SmtOrderManager.Application.Services;

namespace SmtOrderManager.Api.Controllers;

[ApiController]
[Route("api/boards")]
public sealed class BoardsController : ControllerBase
{
    private readonly IBoardService _service;

    public BoardsController(IBoardService service) => _service = service;

    [HttpPost]
    public async Task<ActionResult<BoardDto>> Create([FromBody] CreateBoardRequest request, CancellationToken ct)
        => Ok(await _service.CreateAsync(request, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BoardDto>> GetById(Guid id, CancellationToken ct)
    {
        var dto = await _service.GetByIdAsync(id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BoardDto>>> Search(
    [FromQuery] string? name,
    [FromQuery] int skip = 0,
    [FromQuery] int take = 50,
    CancellationToken ct = default)
    => Ok(await _service.SearchAsync(name, new Paging(skip, take), ct));


    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BoardDto>> Update(Guid id, [FromBody] UpdateBoardRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, request, ct));

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
        => (await _service.DeleteAsync(id, ct)) ? NoContent() : NotFound();

    [HttpPost("{id:guid}/components")]
    public async Task<ActionResult<BoardDto>> AddComponent(Guid id, [FromBody] LinkComponentRequest request, CancellationToken ct)
        => Ok(await _service.AddComponentAsync(id, request, ct));

    [HttpDelete("{id:guid}/components/{componentId:guid}")]
    public async Task<ActionResult<BoardDto>> RemoveComponent(Guid id, Guid componentId, CancellationToken ct)
        => Ok(await _service.RemoveComponentAsync(id, componentId, ct));
}
