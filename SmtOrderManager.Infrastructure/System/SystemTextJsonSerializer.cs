using System.Text.Json;
using SmtOrderManager.Application.Abstractions;

namespace SmtOrderManager.Infrastructure.System;

public sealed class SystemTextJsonSerializer : IJsonSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    public string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);
}
