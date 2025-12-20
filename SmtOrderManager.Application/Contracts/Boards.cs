namespace SmtOrderManager.Application.Contracts;

public sealed record BoardDto(
    Guid Id,
    string Name,
    string Description,
    double Length,
    double Width,
    IReadOnlyList<Guid> ComponentIds
);

public sealed record CreateBoardRequest(
    string Name,
    string Description,
    double Length,
    double Width
);

public sealed record UpdateBoardRequest(
    string Name,
    string Description,
    double Length,
    double Width
);

public sealed record LinkComponentRequest(
    Guid ComponentId,
    int PlacementQuantity
);
