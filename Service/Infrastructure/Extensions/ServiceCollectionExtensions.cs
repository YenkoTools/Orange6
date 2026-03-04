using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Application.Abstractions;
using Application.Interfaces;
using Infrastructure.Configuration;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Services;

namespace Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        if (configuration is not null)
            services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
        else
            services.Configure<DatabaseOptions>(_ => { });

        services.AddSingleton<IDatabaseFactory, DatabaseFactory>();
        services.AddSingleton<DatabaseInitializer>();
        services.AddHostedService<DatabaseInitializerHostedService>();

        services.AddSingleton<IMetricsService, MetricsService>();
        services.AddSingleton<IUserRepository, UserRepository>();

        return services;
    }

}