using Application.Abstractions;
using Application.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddApplicationServices_RegistersCommandDispatcher()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddApplicationServices();
        var provider = services.BuildServiceProvider();

        // Assert
        var dispatcher = provider.GetService<ICommandDispatcher>();
        Assert.NotNull(dispatcher);
    }

    [Fact]
    public void AddApplicationServices_RegistersQueryDispatcher()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddApplicationServices();
        var provider = services.BuildServiceProvider();

        // Assert
        var dispatcher = provider.GetService<IQueryDispatcher>();
        Assert.NotNull(dispatcher);
    }

    [Fact]
    public void AddApplicationServices_RegistersResultAnalyzer()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddApplicationServices();
        var provider = services.BuildServiceProvider();

        // Assert
        var analyzer = provider.GetService<IResultAnalyzer>();
        Assert.NotNull(analyzer);
    }

    [Fact]
    public void AddApplicationServices_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        var result = services.AddApplicationServices();

        // Assert
        Assert.Same(services, result);
    }
}
