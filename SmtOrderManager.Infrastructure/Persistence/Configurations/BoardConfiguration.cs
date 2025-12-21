using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmtOrderManager.Domain.Boards;
using SmtOrderManager.Domain.Orders;

namespace SmtOrderManager.Infrastructure.Persistence.Configurations;

internal sealed class BoardConfiguration : IEntityTypeConfiguration<Board>
{
    public void Configure(EntityTypeBuilder<Board> builder)
    {
        builder.ToTable("Boards");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.Length).IsRequired();
        builder.Property(x => x.Width).IsRequired();

        builder.Navigation(x => x.OrderLinks).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasMany(x => x.OrderLinks)
            .WithOne()
            .HasForeignKey(x => x.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.ComponentLinks).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasMany(x => x.ComponentLinks)
            .WithOne()
            .HasForeignKey(x => x.BoardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
