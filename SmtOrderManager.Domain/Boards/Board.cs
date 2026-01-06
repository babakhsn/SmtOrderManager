using SmtOrderManager.Domain.Common;
using SmtOrderManager.Domain.Orders;

namespace SmtOrderManager.Domain.Boards;

public sealed class Board : Entity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public double Length { get; private set; }
    public double Width { get; private set; }

    //encapsulation and invariants pattern
    private readonly List<OrderBoard> _orderLinks = [];
    public IReadOnlyCollection<OrderBoard> OrderLinks => _orderLinks.AsReadOnly();

    private readonly List<BoardComponent> _componentLinks = [];
    public IReadOnlyCollection<BoardComponent> ComponentLinks => _componentLinks.AsReadOnly();

    private Board()
    {
        Name = string.Empty;
        Description = string.Empty;
    }

    public Board(string name, string description, double length, double width)
    {
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
        Description = description?.Trim() ?? string.Empty;
        Length = Guard.Positive(length, nameof(length));
        Width = Guard.Positive(width, nameof(width));
    }

    public void Update(string name, string description, double length, double width)
    {
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
        Description = description?.Trim() ?? string.Empty;
        Length = Guard.Positive(length, nameof(length));
        Width = Guard.Positive(width, nameof(width));
    }

    public void AddComponent(Guid componentId, int placementQuantity)
    {
        if (_componentLinks.Any(x => x.ComponentId == componentId))
            throw new DomainException("Component already linked to this board.");

        _componentLinks.Add(new BoardComponent(Id, componentId, placementQuantity));
    }

    public void RemoveComponent(Guid componentId)
    {
        var link = _componentLinks.SingleOrDefault(x => x.ComponentId == componentId);
        if (link is null)
            throw new DomainException("Component link not found on this board.");

        _componentLinks.Remove(link);
    }

    public void UpdateComponentQuantity(Guid componentId, int placementQuantity)
    {
        var link = _componentLinks.SingleOrDefault(x => x.ComponentId == componentId);
        if (link is null)
            throw new DomainException("Component link not found on this board.");

        link.UpdateQuantity(placementQuantity);
    }
}
