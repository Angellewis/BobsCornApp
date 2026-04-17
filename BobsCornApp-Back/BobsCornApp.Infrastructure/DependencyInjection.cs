using BobsCornApp.Application.Interfaces;
using BobsCornApp.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace BobsCornApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<ICornPurchaseRepository, CornPurchaseRepository>();

        return services;
    }
}
