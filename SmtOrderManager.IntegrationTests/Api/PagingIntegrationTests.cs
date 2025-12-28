using System.Net;
using System.Net.Http.Json;
using SmtOrderManager.Application.Contracts;
using Xunit;

namespace SmtOrderManager.IntegrationTests.Api;

public sealed class PagingIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PagingIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ComponentsSearch_WithSkipTake_ReturnsPagedResults()
    {
        for (var i = 0; i < 6; i++)
        {
            var resp = await _client.PostAsJsonAsync("/api/components",
                new CreateComponentRequest($"C{i:00}", "d", 1));
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        }

        var list = await _client.GetFromJsonAsync<ComponentDto[]>("/api/components?name=C&skip=2&take=2");
        Assert.NotNull(list);
        Assert.Equal(2, list!.Length);
        Assert.Equal("C02", list[0].Name);
        Assert.Equal("C03", list[1].Name);
    }
}
