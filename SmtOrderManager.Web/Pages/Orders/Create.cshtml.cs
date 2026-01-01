using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmtOrderManager.Application.Contracts;

namespace SmtOrderManager.Web.Pages.Orders;

public sealed class CreateModel : PageModel
{
    private readonly IHttpClientFactory _http;
    public CreateModel(IHttpClientFactory http) => _http = http;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public void OnGet()
    {
        Input.OrderDate = DateTimeOffset.UtcNow;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var client = _http.CreateClient("Api");
        var resp = await client.PostAsJsonAsync("/api/orders",
            new CreateOrderRequest(Input.Name, Input.Description, Input.OrderDate));

        resp.EnsureSuccessStatusCode();
        return RedirectToPage("/Orders/Index");
    }

    public sealed class InputModel
    {
        [Required] public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTimeOffset OrderDate { get; set; }
    }
}
