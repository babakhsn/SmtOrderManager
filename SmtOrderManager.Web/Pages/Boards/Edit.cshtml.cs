using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmtOrderManager.Application.Contracts;

namespace SmtOrderManager.Web.Pages.Boards;

public sealed class EditModel : PageModel
{
    private readonly IHttpClientFactory _http;
    public EditModel(IHttpClientFactory http) => _http = http;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var client = _http.CreateClient("Api");
        var dto = await client.GetFromJsonAsync<BoardDto>($"/api/boards/{Id}");
        if (dto is null) return NotFound();

        Input = new InputModel
        {
            Name = dto.Name,
            Description = dto.Description,
            Length = dto.Length,
            Width = dto.Width
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var client = _http.CreateClient("Api");
        var resp = await client.PutAsJsonAsync($"/api/boards/{Id}",
            new UpdateBoardRequest(Input.Name, Input.Description, Input.Length, Input.Width));

        resp.EnsureSuccessStatusCode();
        return RedirectToPage("/Boards/Index");
    }

    public sealed class InputModel
    {
        [Required] public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        [Range(0.000001, double.MaxValue)] public double Length { get; set; } = 1;
        [Range(0.000001, double.MaxValue)] public double Width { get; set; } = 1;
    }
}
