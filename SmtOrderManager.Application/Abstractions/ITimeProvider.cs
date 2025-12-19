namespace SmtOrderManager.Application.Abstractions;

public interface ITimeProvider
{
    DateTimeOffset UtcNow { get; }
}
