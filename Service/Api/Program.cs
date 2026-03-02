using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Application.Extensions;
using Infrastructure.Extensions;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Application Insights
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// OpenAPI
builder.Services.AddSingleton<IOpenApiConfigurationOptions>(_ => new OpenApiConfigurationOptions
{
    Info = new OpenApiInfo
    {
        Version = DefaultOpenApiConfigurationOptions.GetOpenApiDocVersion(),
        Title = "Orange6 API",
        Description = "Orange6 REST API",
        License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/licenses/MIT"),
        },
    },
    Servers = DefaultOpenApiConfigurationOptions.GetHostNames(),
    OpenApiVersion = DefaultOpenApiConfigurationOptions.GetOpenApiVersion(),
    IncludeRequestingHostName = DefaultOpenApiConfigurationOptions.IsFunctionsRuntimeEnvironmentDevelopment(),
    ForceHttps = DefaultOpenApiConfigurationOptions.IsHttpsForced(),
    ForceHttp = DefaultOpenApiConfigurationOptions.IsHttpForced(),
});

// Application services (dispatchers, handlers, behaviors, validators)
builder.Services.AddApplicationServices();

// Infrastructure services (repositories, metrics)
builder.Services.AddInfrastructure(builder.Configuration);

builder.Build().Run();
