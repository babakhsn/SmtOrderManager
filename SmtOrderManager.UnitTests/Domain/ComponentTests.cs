using SmtOrderManager.Domain.Common;
using SmtOrderManager.Domain.Components;
using Xunit;

namespace SmtOrderManager.IntegrationTests.Domain;

public sealed class ComponentTests
{
    [Fact]
    public void Constructor_WhenQuantityInvalid_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => new Component("C1", "Desc", 0));
    }

    [Fact]
    public void Update_ChangesFields()
    {
        var c = new Component("C1", "Desc", 1);

        c.Update("C2", "New", 5);

        Assert.Equal("C2", c.Name);
        Assert.Equal("New", c.Description);
        Assert.Equal(5, c.Quantity);
    }
}
