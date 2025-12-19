namespace SmtOrderManager.Domain.Orders;

public sealed class OrderBoard
{
    public Guid OrderId { get; private set; }
    public Guid BoardId { get; private set; }

    private OrderBoard() { }

    public OrderBoard(Guid orderId, Guid boardId)
    {
        OrderId = orderId;
        BoardId = boardId;
    }
}
