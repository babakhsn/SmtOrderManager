using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmtOrderManager.Application.Contracts;

namespace SmtOrderManager.Web.Pages.Orders;

public sealed class DeleteModel : PageModel
{
    private readonly IHttpClientFactory _http;
    public DeleteModel(IHttpClientFactory http) => _http = http;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public OrderDto? Item { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var client = _http.CreateClient("Api");
        Item = await client.GetFromJsonAsync<OrderDto>($"/api/orders/{Id}");
        return Item is null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var client = _http.CreateClient("Api");
        var resp = await client.DeleteAsync($"/api/orders/{Id}");
        resp.EnsureSuccessStatusCode();
        return RedirectToPage("/Orders/Index");
    }
}
