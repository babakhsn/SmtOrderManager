using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using SmtOrderManager.Application.Contracts;
using SmtOrderManager.Domain.Boards;
using SmtOrderManager.Domain.Components;
using SmtOrderManager.Domain.Orders;
using SmtOrderManager.Infrastructure.Services;
using Xunit;

namespace SmtOrderManager.IntegrationTests.Infrastructure;

//Infrastructure - application testing (integration)

public sealed class EfOrderServiceDownloadTests
{
    [Fact]
    public async Task DownloadAsync_ProducesBoardsPlacementsAndCorrectBomTotals()
    {
        var (db, connection) = DbContextFactory.CreateSqliteInMemoryContext();
        await using var _ = db;
        await using var __ = connection;

        // Arrange: components
        var r1 = new Component("R_10K", "Resistor 10k", 100);
        var c1 = new Component("C_100n", "Cap 100nF", 200);
        db.Components.AddRange(r1, c1);

        // board with placements: R=2, C=1
        var board = new Board("Board-A", "Main PCB", 100, 50);
        board.AddComponent(r1.Id, 2);
        board.AddComponent(c1.Id, 1);
        db.Boards.Add(board);

        // order includes this board
        var order = new Order("Order-1", "Test", DateTimeOffset.UtcNow);
        order.AddBoard(board.Id);
        db.Orders.Add(order);

        await db.SaveChangesAsync();

        var time = new FakeTimeProvider(new DateTimeOffset(2025, 12, 21, 12, 0, 0, TimeSpan.Zero));
        var json = new TestJsonSerializer();

        var service = new EfOrderService(
            db,
            time,
            json,
            NullLogger<EfOrderService>.Instance
        );

        // Act
        var payload = await service.DownloadAsync(order.Id, CancellationToken.None);

        // Assert: basic payload
        Assert.Equal("application/json", payload.ContentType);
        Assert.Contains(order.Id.ToString(), payload.FileName);

        var jsonText = Encoding.UTF8.GetString(payload.Content);

        // Deserialize download model
        var model = JsonSerializer.Deserialize<ProductionOrderDownloadDto>(
            jsonText,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        Assert.NotNull(model);
        Assert.Equal(time.UtcNow, model!.DownloadedAtUtc);
        Assert.Equal(order.Id, model.Order.Id);

        // Board included with placements
        Assert.Single(model.Boards);
        var b = model.Boards[0];
        Assert.Equal(board.Id, b.Id);
        Assert.Equal(2, b.Placements.Count);

        // Placements contain quantities
        var rPlacement = b.Placements.Single(p => p.ComponentId == r1.Id);
        Assert.Equal(2, rPlacement.PlacementQuantity);

        var cPlacement = b.Placements.Single(p => p.ComponentId == c1.Id);
        Assert.Equal(1, cPlacement.PlacementQuantity);

        // BOM totals match placements (since only one board)
        var bomR = model.Bom.Single(x => x.ComponentId == r1.Id);
        Assert.Equal(2, bomR.TotalQuantity);

        var bomC = model.Bom.Single(x => x.ComponentId == c1.Id);
        Assert.Equal(1, bomC.TotalQuantity);
    }
}
