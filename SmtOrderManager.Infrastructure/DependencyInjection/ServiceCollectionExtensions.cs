using Microsoft.Extensions.DependencyInjection;
using SmtOrderManager.Application.Abstractions;
using SmtOrderManager.Application.Services;
using SmtOrderManager.Infrastructure.InMemory;
using SmtOrderManager.Infrastructure.System;

namespace SmtOrderManager.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<InMemoryStore>();

        services.AddSingleton<ITimeProvider, SystemTimeProvider>();
        services.AddSingleton<IJsonSerializer, SystemTextJsonSerializer>();

        services.AddSingleton<IComponentService, InMemoryComponentService>();
        services.AddSingleton<IBoardService, InMemoryBoardService>();
        services.AddSingleton<IOrderService, InMemoryOrderService>();

        return services;
    }
}
