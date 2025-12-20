namespace SmtOrderManager.Application.Contracts;

public sealed record OrderDto(
    Guid Id,
    string Name,
    string Description,
    DateTimeOffset OrderDate,
    IReadOnlyList<Guid> BoardIds
);

public sealed record CreateOrderRequest(
    string Name,
    string Description,
    DateTimeOffset OrderDate
);

public sealed record UpdateOrderRequest(
    string Name,
    string Description,
    DateTimeOffset OrderDate
);

public sealed record DownloadPayload(
    string FileName,
    string ContentType,
    byte[] Content
);
