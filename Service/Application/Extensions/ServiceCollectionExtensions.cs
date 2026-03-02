using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Application.Abstractions;
using Application.Behaviors;
using Application.Dispatching;
using Application.Features.Users.Commands;
using Application.Features.Users.Queries;
using Domain.Common;
using Domain.Entities;

namespace Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Dispatchers
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();

        // Command handlers
        services.AddScoped<ICommandHandler<CreateUserCommand, Result<User>>, CreateUserCommandHandler>();

        // Query handlers
        services.AddScoped<IQueryHandler<GetUserByIdQuery, Result<User>>, GetUserByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetUsersQuery, Result<PagedResult<User>>>, GetUsersQueryHandler>();

        // Command pipeline: Validation → Performance → Metrics
        services.AddScoped(typeof(ICommandPipelineBehavior<,>), typeof(CommandValidationBehavior<,>));
        services.AddScoped(typeof(ICommandPipelineBehavior<,>), typeof(CommandPerformanceBehavior<,>));
        services.AddScoped(typeof(ICommandPipelineBehavior<,>), typeof(CommandMetricsBehavior<,>));

        // Query pipeline: Validation → Performance → Metrics
        services.AddScoped(typeof(IQueryPipelineBehavior<,>), typeof(QueryValidationBehavior<,>));
        services.AddScoped(typeof(IQueryPipelineBehavior<,>), typeof(QueryPerformanceBehavior<,>));
        services.AddScoped(typeof(IQueryPipelineBehavior<,>), typeof(QueryMetricsBehavior<,>));

        // FluentValidation — register validators from Application assembly
        services.AddValidatorsFromAssembly(typeof(CreateUserCommand).Assembly);

        // Result analyzer
        services.AddSingleton<IResultAnalyzer, DefaultResultAnalyzer>();

        return services;
    }

}