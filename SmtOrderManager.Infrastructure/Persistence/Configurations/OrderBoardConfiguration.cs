using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmtOrderManager.Domain.Boards;
using SmtOrderManager.Domain.Orders;

namespace SmtOrderManager.Infrastructure.Persistence.Configurations;

internal sealed class OrderBoardConfiguration : IEntityTypeConfiguration<OrderBoard>
{
    public void Configure(EntityTypeBuilder<OrderBoard> builder)
    {
        builder.ToTable("OrderBoards");

        builder.HasKey(x => new { x.OrderId, x.BoardId });

        builder.Property(x => x.OrderId).IsRequired();
        builder.Property(x => x.BoardId).IsRequired();

        builder.HasOne<Order>()
            .WithMany()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Board>()
            .WithMany()
            .HasForeignKey(x => x.BoardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
