using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmtOrderManager.Domain.Boards;
using SmtOrderManager.Domain.Components;

namespace SmtOrderManager.Infrastructure.Persistence.Configurations;

internal sealed class BoardComponentConfiguration : IEntityTypeConfiguration<BoardComponent>
{
    public void Configure(EntityTypeBuilder<BoardComponent> builder)
    {
        builder.ToTable("BoardComponents");

        builder.HasKey(x => new { x.BoardId, x.ComponentId });

        builder.Property(x => x.BoardId).IsRequired();
        builder.Property(x => x.ComponentId).IsRequired();
        builder.Property(x => x.PlacementQuantity).IsRequired();

        builder.HasOne<Board>()
            .WithMany()
            .HasForeignKey(x => x.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Component>()
            .WithMany()
            .HasForeignKey(x => x.ComponentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
