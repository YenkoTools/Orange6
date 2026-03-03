using System.Net;
using System.Text.Json;
using Api.Extensions;
using Application.Abstractions;
using FluentValidation;
using Application.Features.Users.Commands;
using Domain.Common;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace Api.Endpoints;

public class UserFunctions(ICommandDispatcher commandDispatcher, ILogger<UserFunctions> logger)
{
    [Function("CreateUser")]
    [OpenApiOperation(operationId: "CreateUser", tags: ["Users"], Summary = "Create a user", Description = "Creates a new user in the system.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(CreateUserCommand), Required = true, Description = "The user to create.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Created, contentType: "application/json", bodyType: typeof(User), Summary = "Created", Description = "The newly created user.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(HttpValidationProblemDetails), Summary = "Bad request", Description = "Validation failed or invalid input.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(ProblemDetails), Summary = "Error", Description = "An unexpected error occurred.")]
    public async Task<IResult> CreateUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "users")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("CreateUser function processed a request.");

        CreateUserCommand? command;
        try
        {
            command = await JsonSerializer.DeserializeAsync<CreateUserCommand>(
                req.Body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                cancellationToken);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Failed to deserialize CreateUserCommand.");
            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request",
                detail: "Invalid request body.");
        }

        if (command is null)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request",
                detail: "Request body is required.");
        }

        Result<User> result;
        try
        {
            result = await commandDispatcher.Dispatch<CreateUserCommand, Result<User>>(command, cancellationToken);
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors
                .GroupBy(f => f.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(f => f.ErrorMessage).ToArray());

            logger.LogWarning("CreateUser validation failed: {Errors}", errors);
            return Results.ValidationProblem(errors);
        }

        logger.LogInformation("CreateUser result: {IsSuccess}", result.IsSuccess);
        return result.IsSuccess
            ? Results.Created($"/api/users/{result.Value.Id}", result.Value)
            : result.ToProblemDetails();
    }
}