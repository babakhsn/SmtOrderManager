using SmtOrderManager.Domain.Common;
using Xunit;

namespace SmtOrderManager.IntegrationTests.Domain;

public sealed class GuardTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NotNullOrWhiteSpace_WhenInvalid_Throws(string? input)
    {
        Assert.Throws<DomainException>(() => Guard.NotNullOrWhiteSpace(input, "name"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void PositiveInt_WhenInvalid_Throws(int value)
    {
        Assert.Throws<DomainException>(() => Guard.Positive(value, "qty"));
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-0.1)]
    public void PositiveDouble_WhenInvalid_Throws(double value)
    {
        Assert.Throws<DomainException>(() => Guard.Positive(value, "len"));
    }
}
