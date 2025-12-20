using System.Collections.Concurrent;
using SmtOrderManager.Domain.Boards;
using SmtOrderManager.Domain.Components;
using SmtOrderManager.Domain.Orders;

namespace SmtOrderManager.Infrastructure.InMemory;

public sealed class InMemoryStore
{
    public ConcurrentDictionary<Guid, Order> Orders { get; } = new();
    public ConcurrentDictionary<Guid, Board> Boards { get; } = new();
    public ConcurrentDictionary<Guid, Component> Components { get; } = new();
}
