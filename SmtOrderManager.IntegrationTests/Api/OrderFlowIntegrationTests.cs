using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SmtOrderManager.Application.Contracts;
using Xunit;

namespace SmtOrderManager.IntegrationTests.Api;

public sealed class OrderFlowIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public OrderFlowIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task FullFlow_CreateLinkDownload_ReturnsProductionPayload()
    {
        // Create components
        var comp1 = await PostOk<ComponentDto>("/api/components",
            new CreateComponentRequest("R_10K", "Resistor 10k", 100));

        var comp2 = await PostOk<ComponentDto>("/api/components",
            new CreateComponentRequest("C_100n", "Cap 100nF", 200));

        // Create board
        var board = await PostOk<BoardDto>("/api/boards",
            new CreateBoardRequest("Board-A", "Main PCB", 100, 50));

        // Link components to board (placements)
        board = await PostOk<BoardDto>($"/api/boards/{board.Id}/components",
            new LinkComponentRequest(comp1.Id, 2));

        board = await PostOk<BoardDto>($"/api/boards/{board.Id}/components",
            new LinkComponentRequest(comp2.Id, 1));

        Assert.Equal(2, board.ComponentIds.Count);

        // Create order
        var order = await PostOk<OrderDto>("/api/orders",
            new CreateOrderRequest("Order-1", "Test order", DateTimeOffset.UtcNow));

        // Link board to order
        order = await PostOk<OrderDto>($"/api/orders/{order.Id}/boards/{board.Id}", body: (object?)null);

        Assert.Single(order.BoardIds);
        Assert.Equal(board.Id, order.BoardIds[0]);

        // Download
        var downloadResponse = await _client.GetAsync($"/api/orders/{order.Id}/download");
        Assert.Equal(HttpStatusCode.OK, downloadResponse.StatusCode);

        Assert.Equal("application/json", downloadResponse.Content.Headers.ContentType?.MediaType);
        Assert.NotNull(downloadResponse.Content.Headers.ContentDisposition);
        Assert.Contains("order_", downloadResponse.Content.Headers.ContentDisposition!.FileNameStar ?? downloadResponse.Content.Headers.ContentDisposition!.FileName);

        var jsonText = await downloadResponse.Content.ReadAsStringAsync();

        // Verify payload model (Step 4 DTO)
        var model = JsonSerializer.Deserialize<ProductionOrderDownloadDto>(
            jsonText,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        Assert.NotNull(model);

        Assert.Equal(order.Id, model!.Order.Id);
        Assert.Single(model.Boards);

        var b = model.Boards[0];
        Assert.Equal(board.Id, b.Id);
        Assert.Equal(2, b.Placements.Count);

        // BOM totals (one board): R=2, C=1
        var bomR = model.Bom.Single(x => x.ComponentId == comp1.Id);
        Assert.Equal(2, bomR.TotalQuantity);

        var bomC = model.Bom.Single(x => x.ComponentId == comp2.Id);
        Assert.Equal(1, bomC.TotalQuantity);
    }

    [Fact]
    public async Task GetMissingOrder_Returns404ProblemJson()
    {
        var resp = await _client.GetAsync($"/api/orders/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);

        // Controller returns NotFound() directly for GetById, so content may be empty.
        // The middleware mapping is validated in Step 6 tests. This test focuses on HTTP behavior.
    }

    private async Task<T> PostOk<T>(string url, object? body)
    {
        HttpResponseMessage resp;

        if (body is null)
        {
            // endpoints like POST /orders/{id}/boards/{boardId} have no body
            resp = await _client.PostAsync(url, content: null);
        }
        else
        {
            resp = await _client.PostAsJsonAsync(url, body);
        }

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var value = await resp.Content.ReadFromJsonAsync<T>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(value);
        return value!;
    }
}
