using Microsoft.Extensions.Logging.Abstractions;
using SmtOrderManager.Application.Contracts;
using SmtOrderManager.Infrastructure.Services;
using Xunit;

namespace SmtOrderManager.IntegrationTests.Infrastructure;

public sealed class PagingTests
{
    [Fact]
    public async Task EfComponentService_SearchAsync_RespectsPaging()
    {
        var (db, connection) = DbContextFactory.CreateSqliteInMemoryContext();
        await using var _ = db;
        await using var __ = connection;

        var svc = new EfComponentService(db, NullLogger<EfComponentService>.Instance);

        // Create 10 components
        for (var i = 0; i < 10; i++)
            await svc.CreateAsync(new CreateComponentRequest($"C{i:00}", "d", 1), CancellationToken.None);

        var page = await svc.SearchAsync(name: "C", paging: new Paging(Skip: 2, Take: 3), CancellationToken.None);

        Assert.Equal(3, page.Count);
        Assert.Equal("C02", page[0].Name); // alphabetical ordering due to padding
        Assert.Equal("C03", page[1].Name);
        Assert.Equal("C04", page[2].Name);
    }
}
