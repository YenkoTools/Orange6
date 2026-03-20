using Application.Abstractions;
using Application.Interfaces;
using Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddInfrastructure_ShouldRegisterIMetricsService()
    {
        var services = new ServiceCollection();

        services.AddInfrastructure(null!);

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IMetricsService));
        Assert.NotNull(descriptor);
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterIUserRepository()
    {
        var services = new ServiceCollection();

        services.AddInfrastructure(null!);

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IUserRepository));
        Assert.NotNull(descriptor);
    }

    [Fact]
    public void AddInfrastructure_IMetricsService_ShouldBeRegisteredAsSingleton()
    {
        var services = new ServiceCollection();

        services.AddInfrastructure(null!);

        var descriptor = services.First(d => d.ServiceType == typeof(IMetricsService));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddInfrastructure_IUserRepository_ShouldBeRegisteredAsSingleton()
    {
        var services = new ServiceCollection();

        services.AddInfrastructure(null!);

        var descriptor = services.First(d => d.ServiceType == typeof(IUserRepository));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddInfrastructure_ShouldReturnServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddInfrastructure(null!);

        Assert.Same(services, result);
    }
}
