using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SmtOrderManager.Web.Pages.Orders;

public sealed class DownloadModel : PageModel
{
    private readonly IHttpClientFactory _http;
    public DownloadModel(IHttpClientFactory http) => _http = http;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; } // OrderId

    public async Task<IActionResult> OnGetAsync()
    {
        var client = _http.CreateClient("Api");

        using var resp = await client.GetAsync($"/api/orders/{Id}/download", HttpCompletionOption.ResponseHeadersRead);
        if (!resp.IsSuccessStatusCode)
            return StatusCode((int)resp.StatusCode);

        var contentType = resp.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
        var fileName =
            resp.Content.Headers.ContentDisposition?.FileNameStar ??
            resp.Content.Headers.ContentDisposition?.FileName ??
            $"order_{Id}.json";

        var bytes = await resp.Content.ReadAsByteArrayAsync();
        return File(bytes, contentType, fileName.Trim('"'));
    }
}
