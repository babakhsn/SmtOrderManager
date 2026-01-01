using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmtOrderManager.Application.Contracts;

namespace SmtOrderManager.Web.Pages.Orders;

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
        var dto = await client.GetFromJsonAsync<OrderDto>($"/api/orders/{Id}");
        if (dto is null) return NotFound();

        Input = new InputModel
        {
            Name = dto.Name,
            Description = dto.Description,
            OrderDate = dto.OrderDate
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var client = _http.CreateClient("Api");
        var resp = await client.PutAsJsonAsync($"/api/orders/{Id}",
            new UpdateOrderRequest(Input.Name, Input.Description, Input.OrderDate));

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
