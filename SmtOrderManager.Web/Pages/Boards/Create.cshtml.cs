using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmtOrderManager.Application.Contracts;

namespace SmtOrderManager.Web.Pages.Boards;

public sealed class CreateModel : PageModel
{
    private readonly IHttpClientFactory _http;
    public CreateModel(IHttpClientFactory http) => _http = http;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var client = _http.CreateClient("Api");
        var resp = await client.PostAsJsonAsync("/api/boards",
            new CreateBoardRequest(Input.Name, Input.Description, Input.Length, Input.Width));

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
