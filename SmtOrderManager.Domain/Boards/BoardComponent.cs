using SmtOrderManager.Domain.Common;

namespace SmtOrderManager.Domain.Boards;

public sealed class BoardComponent
{
    public Guid BoardId { get; private set; }
    public Guid ComponentId { get; private set; }

    /// <summary>
    /// Quantity of this component placed on this board.
    /// </summary>
    public int PlacementQuantity { get; private set; }

    private BoardComponent() { }

    public BoardComponent(Guid boardId, Guid componentId, int placementQuantity)
    {
        BoardId = boardId;
        ComponentId = componentId;
        PlacementQuantity = Guard.Positive(placementQuantity, nameof(placementQuantity));
    }

    public void UpdateQuantity(int placementQuantity)
    {
        PlacementQuantity = Guard.Positive(placementQuantity, nameof(placementQuantity));
    }
}
