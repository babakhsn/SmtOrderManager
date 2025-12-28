using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SmtOrderManager.Application.Contracts;
using Xunit;

namespace SmtOrderManager.IntegrationTests.Api;

public sealed class CascadeAndErrorIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CascadeAndErrorIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DuplicateComponentLink_Returns400ProblemJson()
    {
        // Arrange
        var comp = await PostOk<ComponentDto>("/api/components",
            new CreateComponentRequest("R_10K", "Resistor 10k", 100));

        var board = await PostOk<BoardDto>("/api/boards",
            new CreateBoardRequest("Board-A", "Main PCB", 100, 50));

        // First link should succeed
        _ = await PostOk<BoardDto>($"/api/boards/{board.Id}/components",
            new LinkComponentRequest(comp.Id, 2));

        // Act: link the same component again => DomainException => middleware => 400 ProblemDetails
        var resp = await _client.PostAsJsonAsync($"/api/boards/{board.Id}/components",
            new LinkComponentRequest(comp.Id, 3));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        Assert.Equal("application/problem+json", resp.Content.Headers.ContentType?.MediaType);

        var problem = await resp.Content.ReadFromJsonAsync<ProblemDetailsLike>(JsonOptions);
        Assert.NotNull(problem);
        Assert.Equal(400, problem!.Status);
        Assert.Equal("Domain validation error", problem.Title);
        Assert.False(string.IsNullOrWhiteSpace(problem.Detail));
        Assert.NotNull(problem.TraceId);
    }

    [Fact]
    public async Task DeleteComponent_CascadesPlacements_AndDownloadBomUpdates()
    {
        // Arrange: create components
        var r = await PostOk<ComponentDto>("/api/components",
            new CreateComponentRequest("R_10K", "Resistor 10k", 100));
        var c = await PostOk<ComponentDto>("/api/components",
            new CreateComponentRequest("C_100n", "Cap 100nF", 200));

        // create board and link both components: R=2, C=1
        var board = await PostOk<BoardDto>("/api/boards",
            new CreateBoardRequest("Board-A", "Main PCB", 100, 50));

        _ = await PostOk<BoardDto>($"/api/boards/{board.Id}/components",
            new LinkComponentRequest(r.Id, 2));
        _ = await PostOk<BoardDto>($"/api/boards/{board.Id}/components",
            new LinkComponentRequest(c.Id, 1));

        // create order and link board
        var order = await PostOk<OrderDto>("/api/orders",
            new CreateOrderRequest("Order-1", "Test order", DateTimeOffset.UtcNow));

        order = await PostOk<OrderDto>($"/api/orders/{order.Id}/boards/{board.Id}", body: (object?)null);

        // Verify initial download has both BOM lines
        var initialDownload = await Download(order.Id);
        Assert.Equal(2, initialDownload.Bom.Count);

        // Act: delete component C -> should cascade delete BoardComponents row(s)
        var delResp = await _client.DeleteAsync($"/api/components/{c.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delResp.StatusCode);

        // Assert: download now should have only R in BOM, and board placements reduced
        var afterDelete = await Download(order.Id);

        Assert.Single(afterDelete.Bom);
        Assert.Equal(r.Id, afterDelete.Bom[0].ComponentId);
        Assert.Equal(2, afterDelete.Bom[0].TotalQuantity);

        Assert.Single(afterDelete.Boards);
        var placements = afterDelete.Boards[0].Placements;
        Assert.Single(placements);
        Assert.Equal(r.Id, placements[0].ComponentId);
        Assert.Equal(2, placements[0].PlacementQuantity);
    }

    [Fact]
    public async Task DeleteBoard_CascadesOrderLinks_AndDownloadHasNoBoards()
    {
        // Arrange: component + board with one placement
        var comp = await PostOk<ComponentDto>("/api/components",
            new CreateComponentRequest("U_MCU", "MCU", 10));

        var board = await PostOk<BoardDto>("/api/boards",
            new CreateBoardRequest("Board-A", "Main PCB", 100, 50));

        _ = await PostOk<BoardDto>($"/api/boards/{board.Id}/components",
            new LinkComponentRequest(comp.Id, 1));

        // order links board
        var order = await PostOk<OrderDto>("/api/orders",
            new CreateOrderRequest("Order-1", "Test order", DateTimeOffset.UtcNow));

        _ = await PostOk<OrderDto>($"/api/orders/{order.Id}/boards/{board.Id}", body: (object?)null);

        // Act: delete board -> should cascade OrderBoards + BoardComponents
        var delResp = await _client.DeleteAsync($"/api/boards/{board.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delResp.StatusCode);

        // Assert: order download should now have zero boards and empty BOM
        var download = await Download(order.Id);

        Assert.Empty(download.Boards);
        Assert.Empty(download.Bom);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private async Task<T> PostOk<T>(string url, object? body)
    {
        HttpResponseMessage resp;
        if (body is null)
            resp = await _client.PostAsync(url, content: null);
        else
            resp = await _client.PostAsJsonAsync(url, body);

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var value = await resp.Content.ReadFromJsonAsync<T>(JsonOptions);
        Assert.NotNull(value);
        return value!;
    }

    private async Task<ProductionOrderDownloadDto> Download(Guid orderId)
    {
        var resp = await _client.GetAsync($"/api/orders/{orderId}/download");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equal("application/json", resp.Content.Headers.ContentType?.MediaType);

        var json = await resp.Content.ReadAsStringAsync();
        var model = JsonSerializer.Deserialize<ProductionOrderDownloadDto>(json, JsonOptions);
        Assert.NotNull(model);
        return model!;
    }

    // Minimal ProblemDetails shape to avoid referencing MVC types from tests
    private sealed class ProblemDetailsLike
    {
        public int? Status { get; set; }
        public string? Title { get; set; }
        public string? Detail { get; set; }

        // middleware adds traceId under extensions; we surface it here by custom parsing
        public JsonElement? Extensions { get; set; }

        public string? TraceId
        {
            get
            {
                if (Extensions is null) return null;
                var ext = Extensions.Value;
                if (ext.ValueKind != JsonValueKind.Object) return null;
                if (!ext.TryGetProperty("traceId", out var tid)) return null;
                return tid.GetString();
            }
        }
    }
}
