using Microsoft.AspNetCore.Mvc.RazorPages;
using SmtOrderManager.Application.Contracts;

namespace SmtOrderManager.Web.Pages.Boards;

public sealed class IndexModel : PageModel
{
    private readonly IHttpClientFactory _http;

    public IndexModel(IHttpClientFactory http) => _http = http;

    public List<BoardDto> Items { get; private set; } = new();
    public string? Name { get; private set; }

    public async Task OnGetAsync(string? name)
    {
        Name = name;
        var client = _http.CreateClient("Api");

        var url = string.IsNullOrWhiteSpace(name)
            ? "/api/boards"
            : $"/api/boards?name={Uri.EscapeDataString(name)}";

        Items = (await client.GetFromJsonAsync<List<BoardDto>>(url)) ?? new();
    }
}
