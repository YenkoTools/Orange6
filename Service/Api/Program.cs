using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Orange6.Application.Abstractions;
using Orange6.Application.Behaviors;
using Orange6.Application.Dispatching;
using Orange6.Application.Features.Users.Commands;
using Orange6.Application.Features.Users.Queries;
using Orange6.Application.Interfaces;
using Orange6.Domain.Common;
using Orange6.Domain.Entities;
using Orange6.Infrastructure.Repositories;
using Orange6.Infrastructure.Services;

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

// Dispatchers
builder.Services.AddScoped<ICommandDispatcher, CommandDispatcher>();
builder.Services.AddScoped<IQueryDispatcher, QueryDispatcher>();

// Infrastructure
builder.Services.AddSingleton<IMetricsService, MetricsService>();
builder.Services.AddSingleton<IResultAnalyzer, DefaultResultAnalyzer>();
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();

// Command handlers
builder.Services.AddScoped<ICommandHandler<CreateUserCommand, Result<User>>, CreateUserCommandHandler>();

// Query handlers
builder.Services.AddScoped<IQueryHandler<GetUserByIdQuery, Result<User>>, GetUserByIdQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetUsersQuery, Result<PagedResult<User>>>, GetUsersQueryHandler>();

// Command pipeline: Validation → Performance → Metrics
builder.Services.AddScoped(typeof(ICommandPipelineBehavior<,>), typeof(CommandValidationBehavior<,>));
builder.Services.AddScoped(typeof(ICommandPipelineBehavior<,>), typeof(CommandPerformanceBehavior<,>));
builder.Services.AddScoped(typeof(ICommandPipelineBehavior<,>), typeof(CommandMetricsBehavior<,>));

// Query pipeline: Validation → Performance → Metrics
builder.Services.AddScoped(typeof(IQueryPipelineBehavior<,>), typeof(QueryValidationBehavior<,>));
builder.Services.AddScoped(typeof(IQueryPipelineBehavior<,>), typeof(QueryPerformanceBehavior<,>));
builder.Services.AddScoped(typeof(IQueryPipelineBehavior<,>), typeof(QueryMetricsBehavior<,>));

// FluentValidation — register validators from Application assembly
builder.Services.AddValidatorsFromAssembly(typeof(CreateUserCommand).Assembly);

builder.Build().Run();
