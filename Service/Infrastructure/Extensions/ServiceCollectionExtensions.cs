using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Application.Abstractions;
using Application.Interfaces;
using Infrastructure.Repositories;
using Infrastructure.Services;

namespace Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IMetricsService, MetricsService>();
        services.AddSingleton<IUserRepository, InMemoryUserRepository>();

        return services;
    }

}