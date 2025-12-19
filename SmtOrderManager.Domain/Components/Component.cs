using SmtOrderManager.Domain.Common;
using SmtOrderManager.Domain.Boards;

namespace SmtOrderManager.Domain.Components;

public sealed class Component : Entity
{
    public string Name { get; private set; }
    public string Description { get; private set; }

    /// <summary>
    /// Default quantity attribute required by the challenge.
    /// In practice, placement quantity is typically per-board, which we’ll model in BoardComponent.
    /// </summary>
    public int Quantity { get; private set; }

    private readonly List<BoardComponent> _boardLinks = [];
    public IReadOnlyCollection<BoardComponent> BoardLinks => _boardLinks.AsReadOnly();

    private Component() // for ORM/serialization later
    {
        Name = string.Empty;
        Description = string.Empty;
    }

    public Component(string name, string description, int quantity)
    {
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
        Description = description?.Trim() ?? string.Empty;
        Quantity = Guard.Positive(quantity, nameof(quantity));
    }

    public void Update(string name, string description, int quantity)
    {
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
        Description = description?.Trim() ?? string.Empty;
        Quantity = Guard.Positive(quantity, nameof(quantity));
    }
}
