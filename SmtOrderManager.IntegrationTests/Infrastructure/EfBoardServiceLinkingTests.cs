using Microsoft.Extensions.Logging.Abstractions;
using SmtOrderManager.Application.Contracts;
using SmtOrderManager.Domain.Common;
using SmtOrderManager.Infrastructure.Services;
using Xunit;

namespace SmtOrderManager.IntegrationTests.Infrastructure;

public sealed class EfBoardServiceLinkingTests
{
    [Fact]
    public async Task EfBoardService_AddComponentAsync_WhenLinkedTwice_ThrowsDomainException()
    {
        var (db, connection) = DbContextFactory.CreateSqliteInMemoryContext();
        await using var _ = db;
        await using var __ = connection;

        var componentService = new EfComponentService(db, NullLogger<EfComponentService>.Instance);
        var boardService = new EfBoardService(db, NullLogger<EfBoardService>.Instance);

        var comp = await componentService.CreateAsync(new CreateComponentRequest("R1", "Res", 10), CancellationToken.None);
        var board = await boardService.CreateAsync(new CreateBoardRequest("B1", "Board", 10, 5), CancellationToken.None);

        await boardService.AddComponentAsync(board.Id, new LinkComponentRequest(comp.Id, 2), CancellationToken.None);

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            boardService.AddComponentAsync(board.Id, new LinkComponentRequest(comp.Id, 3), CancellationToken.None));

        Assert.Contains("already linked", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
