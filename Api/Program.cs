using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource("Orange6.Api.Commands")
        .AddSource("Orange6.Api.Queries")
        .AddAspNetCoreInstrumentation())
    .WithMetrics(metrics => metrics
        .AddMeter("Orange6.Api.Commands")
        .AddMeter("Orange6.Api.Queries"));

// When APPLICATIONINSIGHTS_CONNECTION_STRING is set, export telemetry to Azure Monitor.
if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
{
    builder.Services.AddOpenTelemetry().UseAzureMonitor();
}

builder.Build().Run();
