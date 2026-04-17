using BobsCornApp.Application.Interfaces;
using BobsCornApp.Application.Mapping;
using BobsCornApp.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BobsCornApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(_ => { }, typeof(CornMappingProfile).Assembly);
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<ICornPurchaseService, CornPurchaseService>();

        return services;
    }
}
