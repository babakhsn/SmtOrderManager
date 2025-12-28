using SmtOrderManager.Domain.Boards;
using SmtOrderManager.Domain.Common;
using Xunit;

namespace SmtOrderManager.IntegrationTests.Domain;

public sealed class BoardTests
{
    [Fact]
    public void AddComponent_WhenAlreadyLinked_ThrowsDomainException()
    {
        var board = new Board("B1", "Board", 10, 5);
        var componentId = Guid.NewGuid();

        board.AddComponent(componentId, placementQuantity: 2);

        var ex = Assert.Throws<DomainException>(() => board.AddComponent(componentId, placementQuantity: 3));
        Assert.Contains("already linked", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void UpdateComponentQuantity_WhenLinkExists_UpdatesQuantity()
    {
        var board = new Board("B1", "Board", 10, 5);
        var componentId = Guid.NewGuid();

        board.AddComponent(componentId, placementQuantity: 2);
        board.UpdateComponentQuantity(componentId, placementQuantity: 7);

        var link = board.ComponentLinks.Single(x => x.ComponentId == componentId);
        Assert.Equal(7, link.PlacementQuantity);
    }

    [Fact]
    public void RemoveComponent_WhenNotLinked_ThrowsDomainException()
    {
        var board = new Board("B1", "Board", 10, 5);

        var ex = Assert.Throws<DomainException>(() => board.RemoveComponent(Guid.NewGuid()));
        Assert.Contains("not found", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
