using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmtOrderManager.Application.Contracts;

namespace SmtOrderManager.Web.Pages.Components;

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
        var resp = await client.PostAsJsonAsync("/api/components",
            new CreateComponentRequest(Input.Name, Input.Description, Input.Quantity));

        resp.EnsureSuccessStatusCode();
        return RedirectToPage("/Components/Index");
    }

    public sealed class InputModel
    {
        [Required] public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [Range(1, int.MaxValue)] public int Quantity { get; set; } = 1;
    }
}
