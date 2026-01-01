using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmtOrderManager.Application.Contracts;

namespace SmtOrderManager.Web.Pages.Orders;

public sealed class BoardsModel : PageModel
{
    private readonly IHttpClientFactory _http;
    public BoardsModel(IHttpClientFactory http) => _http = http;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; } // OrderId

    public OrderDto? Order { get; private set; }
    public List<BoardDto> AllBoards { get; private set; } = new();

    [BindProperty]
    public LinkInputModel LinkInput { get; set; } = new();

    [BindProperty]
    public Guid RemoveBoardId { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var client = _http.CreateClient("Api");

        Order = await client.GetFromJsonAsync<OrderDto>($"/api/orders/{Id}");
        if (Order is null) return NotFound();

        AllBoards = (await client.GetFromJsonAsync<List<BoardDto>>("/api/boards")) ?? new();
        return Page();
    }

    public async Task<IActionResult> OnPostLinkAsync()
    {
        if (!ModelState.IsValid) return await OnGetAsync();

        var client = _http.CreateClient("Api");

        var resp = await client.PostAsync($"/api/orders/{Id}/boards/{LinkInput.BoardId}", content: null);
        resp.EnsureSuccessStatusCode();

        return RedirectToPage("/Orders/Boards", new { id = Id });
    }

    public async Task<IActionResult> OnPostRemoveAsync()
    {
        var client = _http.CreateClient("Api");

        var resp = await client.DeleteAsync($"/api/orders/{Id}/boards/{RemoveBoardId}");
        resp.EnsureSuccessStatusCode();

        return RedirectToPage("/Orders/Boards", new { id = Id });
    }

    public sealed class LinkInputModel
    {
        [Required]
        public Guid BoardId { get; set; }
    }
}
