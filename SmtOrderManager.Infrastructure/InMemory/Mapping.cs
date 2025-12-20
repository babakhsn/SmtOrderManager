using SmtOrderManager.Application.Contracts;
using SmtOrderManager.Domain.Boards;
using SmtOrderManager.Domain.Components;
using SmtOrderManager.Domain.Orders;

namespace SmtOrderManager.Infrastructure.InMemory;

internal static class Mapping
{
    public static ComponentDto ToDto(Component c) =>
        new(c.Id, c.Name, c.Description, c.Quantity);

    public static BoardDto ToDto(Board b) =>
        new(
            b.Id,
            b.Name,
            b.Description,
            b.Length,
            b.Width,
            b.ComponentLinks.Select(x => x.ComponentId).ToList()
        );

    public static OrderDto ToDto(Order o) =>
        new(
            o.Id,
            o.Name,
            o.Description,
            o.OrderDate,
            o.BoardLinks.Select(x => x.BoardId).ToList()
        );
}
