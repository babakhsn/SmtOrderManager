using Microsoft.AspNetCore.Mvc;
using SmtOrderManager.Application.Contracts;
using SmtOrderManager.Application.Services;

namespace SmtOrderManager.Api.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderService _service;

    public OrdersController(IOrderService service) => _service = service;

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderRequest request, CancellationToken ct)
        => Ok(await _service.CreateAsync(request, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetById(Guid id, CancellationToken ct)
    {
        var dto = await _service.GetByIdAsync(id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> Search(
    [FromQuery] string? name,
    [FromQuery] int skip = 0,
    [FromQuery] int take = 50,
    CancellationToken ct = default)
    => Ok(await _service.SearchAsync(name, new Paging(skip, take), ct));


    [HttpPut("{id:guid}")]
    public async Task<ActionResult<OrderDto>> Update(Guid id, [FromBody] UpdateOrderRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, request, ct));

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
        => (await _service.DeleteAsync(id, ct)) ? NoContent() : NotFound();

    [HttpPost("{id:guid}/boards/{boardId:guid}")]
    public async Task<ActionResult<OrderDto>> AddBoard(Guid id, Guid boardId, CancellationToken ct)
        => Ok(await _service.AddBoardAsync(id, boardId, ct));

    [HttpDelete("{id:guid}/boards/{boardId:guid}")]
    public async Task<ActionResult<OrderDto>> RemoveBoard(Guid id, Guid boardId, CancellationToken ct)
        => Ok(await _service.RemoveBoardAsync(id, boardId, ct));

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        var payload = await _service.DownloadAsync(id, ct);
        return File(payload.Content, payload.ContentType, payload.FileName);
    }
}
