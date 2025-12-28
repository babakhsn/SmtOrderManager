using Microsoft.Extensions.Logging.Abstractions;
using SmtOrderManager.Application.Contracts;
using SmtOrderManager.Domain.Common;
using SmtOrderManager.Infrastructure.Services;
using Xunit;

namespace SmtOrderManager.IntegrationTests.Infrastructure;

public sealed class EfServiceNotFoundTests
{
    [Fact]
    public async Task EfOrderService_UpdateAsync_WhenOrderMissing_ThrowsNotFoundException()
    {
        var (db, connection) = DbContextFactory.CreateSqliteInMemoryContext();
        await using var _ = db;
        await using var __ = connection;

        var service = new EfOrderService(
            db,
            new FakeTimeProvider(DateTimeOffset.UtcNow),
            new TestJsonSerializer(),
            NullLogger<EfOrderService>.Instance
        );

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UpdateAsync(Guid.NewGuid(), new UpdateOrderRequest("n", "d", DateTimeOffset.UtcNow), CancellationToken.None));
    }

    [Fact]
    public async Task EfBoardService_AddComponentAsync_WhenBoardMissing_ThrowsNotFoundException()
    {
        var (db, connection) = DbContextFactory.CreateSqliteInMemoryContext();
        await using var _ = db;
        await using var __ = connection;

        var service = new EfBoardService(db, NullLogger<EfBoardService>.Instance);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.AddComponentAsync(Guid.NewGuid(), new LinkComponentRequest(Guid.NewGuid(), 1), CancellationToken.None));
    }

    [Fact]
    public async Task EfBoardService_AddComponentAsync_WhenComponentMissing_ThrowsNotFoundException()
    {
        var (db, connection) = DbContextFactory.CreateSqliteInMemoryContext();
        await using var _ = db;
        await using var __ = connection;

        // Create only the board
        var boardService = new EfBoardService(db, NullLogger<EfBoardService>.Instance);

        var board = await boardService.CreateAsync(
            new CreateBoardRequest("B1", "Board", 10, 5),
            CancellationToken.None
        );

        await Assert.ThrowsAsync<NotFoundException>(() =>
            boardService.AddComponentAsync(board.Id, new LinkComponentRequest(Guid.NewGuid(), 1), CancellationToken.None));
    }

    [Fact]
    public async Task EfOrderService_AddBoardAsync_WhenBoardMissing_ThrowsNotFoundException()
    {
        var (db, connection) = DbContextFactory.CreateSqliteInMemoryContext();
        await using var _ = db;
        await using var __ = connection;

        var orderService = new EfOrderService(
            db,
            new FakeTimeProvider(DateTimeOffset.UtcNow),
            new TestJsonSerializer(),
            NullLogger<EfOrderService>.Instance
        );

        var order = await orderService.CreateAsync(
            new CreateOrderRequest("O1", "Order", DateTimeOffset.UtcNow),
            CancellationToken.None
        );

        await Assert.ThrowsAsync<NotFoundException>(() =>
            orderService.AddBoardAsync(order.Id, Guid.NewGuid(), CancellationToken.None));
    }
}
