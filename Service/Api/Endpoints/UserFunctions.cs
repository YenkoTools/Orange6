using System.Net;
using System.Text.Json;
using Api.Extensions;
using Application.Abstractions;
using FluentValidation;
using Application.Features.Users.Commands;
using Application.Features.Users.Queries;
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

public class UserFunctions(ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher, ILogger<UserFunctions> logger)
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

    [Function("UpdateUser")]
    [OpenApiOperation(operationId: "UpdateUser", tags: ["Users"], Summary = "Update a user", Description = "Updates an existing user in the system. The user id is taken from the route parameter.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "The unique identifier of the user to update.")]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(UpdateUserCommand), Required = true, Description = "The updated user data.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(User), Summary = "OK", Description = "The updated user.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(HttpValidationProblemDetails), Summary = "Bad request", Description = "Validation failed or invalid input.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(ProblemDetails), Summary = "Not found", Description = "The user was not found.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(ProblemDetails), Summary = "Error", Description = "An unexpected error occurred.")]
    public async Task<IResult> UpdateUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "users/{id:int}")] HttpRequest req,
        int id,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("UpdateUser function processed a request.");

        UpdateUserCommand? command;
        try
        {
            command = await JsonSerializer.DeserializeAsync<UpdateUserCommand>(
                req.Body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                cancellationToken);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Failed to deserialize UpdateUserCommand.");
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

        command = command with { Id = id };

        Result<User> result;
        try
        {
            result = await commandDispatcher.Dispatch<UpdateUserCommand, Result<User>>(command, cancellationToken);
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors
                .GroupBy(f => f.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(f => f.ErrorMessage).ToArray());

            logger.LogWarning("UpdateUser validation failed: {Errors}", errors);
            return Results.ValidationProblem(errors);
        }

        logger.LogInformation("UpdateUser result: {IsSuccess}", result.IsSuccess);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : result.ToProblemDetails();
    }

    [Function("GetUsers")]
    [OpenApiOperation(operationId: "GetUsers", tags: ["Users"], Summary = "Get users", Description = "Retrieves a paginated list of users.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "pageNumber", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "The page number (1-based). Defaults to 1.")]
    [OpenApiParameter(name: "pageSize", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "The number of items per page. Defaults to 10.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(PagedResult<User>), Summary = "OK", Description = "A paginated list of users.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(ProblemDetails), Summary = "Not found", Description = "No users were found.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(ProblemDetails), Summary = "Error", Description = "An unexpected error occurred.")]
    public async Task<IResult> GetUsers(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("GetUsers function processed a request.");

        int pageNumber = int.TryParse(req.Query["pageNumber"], out var pn) ? pn : 1;
        int pageSize = int.TryParse(req.Query["pageSize"], out var ps) ? ps : 10;
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Max(1, pageSize);

        var query = new GetUsersQuery(pageNumber, pageSize);

        var result = await queryDispatcher.Dispatch<GetUsersQuery, Result<PagedResult<User>>>(query, cancellationToken);

        logger.LogInformation("GetUsers result: {IsSuccess}", result.IsSuccess);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : result.ToProblemDetails();
    }

    [Function("DeleteUser")]
    [OpenApiOperation(operationId: "DeleteUser", tags: ["Users"], Summary = "Delete a user", Description = "Deletes an existing user from the system by their unique identifier.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "The unique identifier of the user to delete.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "No content", Description = "The user was successfully deleted.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(ProblemDetails), Summary = "Not found", Description = "The user was not found.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(ProblemDetails), Summary = "Error", Description = "An unexpected error occurred.")]
    public async Task<IResult> DeleteUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "users/{id:int}")] HttpRequest req,
        int id,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("DeleteUser function processed a request.");

        var command = new DeleteUserCommand(id);

        Result result;
        try
        {
            result = await commandDispatcher.Dispatch<DeleteUserCommand, Result>(command, cancellationToken);
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors
                .GroupBy(f => f.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(f => f.ErrorMessage).ToArray());

            logger.LogWarning("DeleteUser validation failed: {Errors}", errors);
            return Results.ValidationProblem(errors);
        }

        logger.LogInformation("DeleteUser result: {IsSuccess}", result.IsSuccess);
        return result.IsSuccess
            ? Results.NoContent()
            : result.ToProblemDetails();
    }
}