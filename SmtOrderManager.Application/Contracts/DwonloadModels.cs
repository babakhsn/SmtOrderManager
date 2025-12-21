namespace SmtOrderManager.Application.Contracts;

public sealed record ProductionOrderDownloadDto(
    DateTimeOffset DownloadedAtUtc,
    OrderDownloadDto Order,
    IReadOnlyList<BoardDownloadDto> Boards,
    IReadOnlyList<BomLineDto> Bom
);

public sealed record OrderDownloadDto(
    Guid Id,
    string Name,
    string Description,
    DateTimeOffset OrderDate
);

public sealed record BoardDownloadDto(
    Guid Id,
    string Name,
    string Description,
    double Length,
    double Width,
    IReadOnlyList<BoardPlacementDto> Placements
);

public sealed record BoardPlacementDto(
    Guid ComponentId,
    string ComponentName,
    string ComponentDescription,
    int PlacementQuantity
);

public sealed record BomLineDto(
    Guid ComponentId,
    string ComponentName,
    int TotalQuantity
);
