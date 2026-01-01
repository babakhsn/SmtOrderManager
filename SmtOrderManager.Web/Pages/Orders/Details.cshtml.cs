using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmtOrderManager.Application.Contracts;

namespace SmtOrderManager.Web.Pages.Orders;

public sealed class DetailsModel : PageModel
{
    private readonly IHttpClientFactory _http;

    public DetailsModel(IHttpClientFactory http) => _http = http;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; } // OrderId

    public ProductionOrderDownloadDto? ModelData { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var client = _http.CreateClient("Api");

        using var resp = await client.GetAsync($"/api/orders/{Id}/download", HttpCompletionOption.ResponseHeadersRead);
        if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
            return NotFound();

        if (!resp.IsSuccessStatusCode)
            return StatusCode((int)resp.StatusCode);

        var json = await resp.Content.ReadAsStringAsync();

        ModelData = JsonSerializer.Deserialize<ProductionOrderDownloadDto>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (ModelData is null)
            return StatusCode(500);

        return Page();
    }
}
