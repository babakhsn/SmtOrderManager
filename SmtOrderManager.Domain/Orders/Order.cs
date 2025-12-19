using SmtOrderManager.Domain.Common;

namespace SmtOrderManager.Domain.Orders;

public sealed class Order : Entity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public DateTimeOffset OrderDate { get; private set; }

    private readonly List<OrderBoard> _boardLinks = [];
    public IReadOnlyCollection<OrderBoard> BoardLinks => _boardLinks.AsReadOnly();

    private Order()
    {
        Name = string.Empty;
        Description = string.Empty;
    }

    public Order(string name, string description, DateTimeOffset orderDate)
    {
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
        Description = description?.Trim() ?? string.Empty;
        OrderDate = orderDate;
    }

    public void Update(string name, string description, DateTimeOffset orderDate)
    {
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
        Description = description?.Trim() ?? string.Empty;
        OrderDate = orderDate;
    }

    public void AddBoard(Guid boardId)
    {
        if (_boardLinks.Any(x => x.BoardId == boardId))
            throw new DomainException("Board already linked to this order.");

        _boardLinks.Add(new OrderBoard(Id, boardId));
    }

    public void RemoveBoard(Guid boardId)
    {
        var link = _boardLinks.SingleOrDefault(x => x.BoardId == boardId);
        if (link is null)
            throw new DomainException("Board link not found on this order.");

        _boardLinks.Remove(link);
    }
}
