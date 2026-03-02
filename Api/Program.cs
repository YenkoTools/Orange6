using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

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

builder.Build().Run();
