using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmtOrderManager.Domain.Components;

namespace SmtOrderManager.Infrastructure.Persistence.Configurations;

internal sealed class ComponentConfiguration : IEntityTypeConfiguration<Component>
{
    public void Configure(EntityTypeBuilder<Component> builder)
    {
        builder.ToTable("Components");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.Quantity).IsRequired();

        builder.Navigation(x => x.BoardLinks).UsePropertyAccessMode(PropertyAccessMode.Field);

        // Collection exists on Component, but join entity lives in Boards namespace.
        // Configure relationship from Component side via FK on BoardComponent.ComponentId:
        builder.HasMany(x => x.BoardLinks)
            .WithOne()
            .HasForeignKey("ComponentId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
