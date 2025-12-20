namespace SmtOrderManager.Application.Contracts;

public sealed record ComponentDto(
    Guid Id,
    string Name,
    string Description,
    int Quantity
);

public sealed record CreateComponentRequest(
    string Name,
    string Description,
    int Quantity
);

public sealed record UpdateComponentRequest(
    string Name,
    string Description,
    int Quantity
);
