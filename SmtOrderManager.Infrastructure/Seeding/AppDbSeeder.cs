// SmtOrderManager.Infrastructure/Seeding/AppDbSeeder.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Application.Contracts;
using SmtOrderManager.Application.Services;
using SmtOrderManager.Infrastructure.Persistence;

namespace SmtOrderManager.Infrastructure.Seeding;

public static class AppDbSeeder
{
    /// <summary>
    /// Idempotent seed: does nothing if any Orders/Boards/Components already exist.
    /// Uses application services to stay consistent with domain rules and existing logic.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();

        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("AppDbSeeder");

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Idempotency: if any data exists, don't seed.
        if (await db.Components.AsNoTracking().AnyAsync(ct) ||
            await db.Boards.AsNoTracking().AnyAsync(ct) ||
            await db.Orders.AsNoTracking().AnyAsync(ct))
        {
            logger.LogInformation("Seeding skipped (database already contains data).");
            return;
        }

        var componentService = scope.ServiceProvider.GetRequiredService<IComponentService>();
        var boardService = scope.ServiceProvider.GetRequiredService<IBoardService>();
        var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

        logger.LogInformation("Seeding database with demo data...");

        // Components
        var r10k = await componentService.CreateAsync(
            new CreateComponentRequest("R_10K", "Resistor 10k", 5000),
            ct);

        var c100n = await componentService.CreateAsync(
            new CreateComponentRequest("C_100N", "Capacitor 100nF", 8000),
            ct);

        var mcu = await componentService.CreateAsync(
            new CreateComponentRequest("U_MCU", "Microcontroller", 200),
            ct);

        // Boards
        var boardA = await boardService.CreateAsync(
            new CreateBoardRequest("Board-A", "Main PCB", 100, 50),
            ct);

        var boardB = await boardService.CreateAsync(
            new CreateBoardRequest("Board-B", "IO PCB", 80, 40),
            ct);

        // Board ↔ Component linking (with placement quantity)
        await boardService.AddComponentAsync(boardA.Id, new LinkComponentRequest(r10k.Id, 4), ct);
        await boardService.AddComponentAsync(boardA.Id, new LinkComponentRequest(c100n.Id, 2), ct);
        await boardService.AddComponentAsync(boardA.Id, new LinkComponentRequest(mcu.Id, 1), ct);

        await boardService.AddComponentAsync(boardB.Id, new LinkComponentRequest(r10k.Id, 2), ct);
        await boardService.AddComponentAsync(boardB.Id, new LinkComponentRequest(c100n.Id, 1), ct);

        // Order
        var order = await orderService.CreateAsync(
            new CreateOrderRequest("Order-001", "Seeded demo order", DateTimeOffset.UtcNow),
            ct);

        // Order ↔ Board linking
        await orderService.AddBoardAsync(order.Id, boardA.Id, ct);
        await orderService.AddBoardAsync(order.Id, boardB.Id, ct);

        logger.LogInformation(
            "Seeding finished. OrderId={OrderId}, Boards={BoardCount}, Components={ComponentCount}",
            order.Id, 2, 3);
    }
}
