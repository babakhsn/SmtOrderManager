using SmtOrderManager.Domain.Common;
using SmtOrderManager.Domain.Orders;
using Xunit;

namespace SmtOrderManager.IntegrationTests.Domain;

public sealed class OrderTests
{
    [Fact]
    public void AddBoard_WhenBoardAlreadyLinked_ThrowsDomainException()
    {
        // Arrange
        var order = new Order(
            name: "Order-001",
            description: "Test order",
            orderDate: DateTimeOffset.UtcNow
        );

        var boardId = Guid.NewGuid();

        // Act
        order.AddBoard(boardId);

        // Assert
        var ex = Assert.Throws<DomainException>(() => order.AddBoard(boardId));
        Assert.Contains("already linked", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
