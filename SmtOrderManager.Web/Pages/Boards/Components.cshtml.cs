using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmtOrderManager.Application.Contracts;

namespace SmtOrderManager.Web.Pages.Boards;

public sealed class ComponentsModel : PageModel
{
    private readonly IHttpClientFactory _http;
    public ComponentsModel(IHttpClientFactory http) => _http = http;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; } // BoardId

    public BoardDto? Board { get; private set; }
    public List<ComponentDto> AllComponents { get; private set; } = new();

    [BindProperty]
    public LinkInputModel LinkInput { get; set; } = new();

    [BindProperty]
    public Guid RemoveComponentId { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var client = _http.CreateClient("Api");

        Board = await client.GetFromJsonAsync<BoardDto>($"/api/boards/{Id}");
        if (Board is null) return NotFound();

        AllComponents = (await client.GetFromJsonAsync<List<ComponentDto>>("/api/components")) ?? new();
        return Page();
    }

    public async Task<IActionResult> OnPostLinkAsync()
    {
        if (!ModelState.IsValid) return await OnGetAsync();

        var client = _http.CreateClient("Api");

        var resp = await client.PostAsJsonAsync($"/api/boards/{Id}/components",
            new LinkComponentRequest(LinkInput.ComponentId, LinkInput.PlacementQuantity));

        resp.EnsureSuccessStatusCode();
        return RedirectToPage("/Boards/Components", new { id = Id });
    }

    public async Task<IActionResult> OnPostRemoveAsync()
    {
        var client = _http.CreateClient("Api");

        var resp = await client.DeleteAsync($"/api/boards/{Id}/components/{RemoveComponentId}");
        resp.EnsureSuccessStatusCode();

        return RedirectToPage("/Boards/Components", new { id = Id });
    }

    public sealed class LinkInputModel
    {
        [Required]
        public Guid ComponentId { get; set; }

        [Range(1, int.MaxValue)]
        public int PlacementQuantity { get; set; } = 1;
    }
}
