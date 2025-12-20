using SmtOrderManager.Application.Abstractions;

namespace SmtOrderManager.Infrastructure.System;

public sealed class SystemTimeProvider : ITimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
