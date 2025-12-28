using System.Text.Json;
using SmtOrderManager.Application.Abstractions;

namespace SmtOrderManager.IntegrationTests.Infrastructure;

internal sealed class FakeTimeProvider : ITimeProvider
{
    public DateTimeOffset UtcNow { get; }

    public FakeTimeProvider(DateTimeOffset utcNow) => UtcNow = utcNow;
}

internal sealed class TestJsonSerializer : IJsonSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);
}
