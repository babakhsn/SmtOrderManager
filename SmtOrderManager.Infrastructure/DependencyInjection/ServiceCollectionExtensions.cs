using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmtOrderManager.Application.Abstractions;
using SmtOrderManager.Application.Services;
using SmtOrderManager.Infrastructure.Persistence;
using SmtOrderManager.Infrastructure.Services;
using SmtOrderManager.Infrastructure.System;

namespace SmtOrderManager.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ITimeProvider, SystemTimeProvider>();
        services.AddSingleton<IJsonSerializer, SystemTextJsonSerializer>();

        var connStr = configuration.GetConnectionString("AppDb");
        services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(connStr));

        services.AddScoped<IComponentService, EfComponentService>();
        services.AddScoped<IBoardService, EfBoardService>();
        services.AddScoped<IOrderService, EfOrderService>();

        return services;
    }
}
