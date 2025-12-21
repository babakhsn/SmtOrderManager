using Microsoft.EntityFrameworkCore;
using SmtOrderManager.Domain.Boards;
using SmtOrderManager.Domain.Components;
using SmtOrderManager.Domain.Orders;

namespace SmtOrderManager.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Board> Boards => Set<Board>();
    public DbSet<Component> Components => Set<Component>();

    public DbSet<OrderBoard> OrderBoards => Set<OrderBoard>();
    public DbSet<BoardComponent> BoardComponents => Set<BoardComponent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
