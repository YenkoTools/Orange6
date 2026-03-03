using System.Text;
using System.Text.Json;
using Api.Endpoints;
using Application.Abstractions;
using Application.Features.Users.Commands;
using Domain.Common;
using Domain.Entities;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Api.Tests;

public class UserFunctionsTests
{
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly ILogger<UserFunctions> _logger;
    private readonly UserFunctions _sut;

    public UserFunctionsTests()
    {
        _commandDispatcher = Substitute.For<ICommandDispatcher>();
        _logger = Substitute.For<ILogger<UserFunctions>>();
        _sut = new UserFunctions(_commandDispatcher, _logger);
    }

    private static HttpRequest CreateRequestWithBody(object body)
    {
        var json = JsonSerializer.Serialize(body);
        var context = new DefaultHttpContext();
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
        context.Request.ContentType = "application/json";
        return context.Request;
    }

    private static HttpRequest CreateRequestWithRawBody(string rawBody)
    {
        var context = new DefaultHttpContext();
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(rawBody));
        context.Request.ContentType = "application/json";
        return context.Request;
    }

    private static HttpRequest CreateEmptyRequest()
    {
        var context = new DefaultHttpContext();
        context.Request.Body = Stream.Null;
        return context.Request;
    }

    // ── CreateUser ──────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateUser_ReturnsCreated_WhenCommandSucceeds()
    {
        var command = new CreateUserCommand("jdoe", "jdoe@test.com", "John", "Doe");
        var user = new User { Id = 1, Username = "jdoe", Email = "jdoe@test.com", FirstName = "John", LastName = "Doe" };

        _commandDispatcher
            .Dispatch<CreateUserCommand, Result<User>>(Arg.Any<CreateUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<User>.Success(user));

        var result = await _sut.CreateUser(CreateRequestWithBody(command), CancellationToken.None);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status201Created, statusResult.StatusCode);
    }

    [Fact]
    public async Task CreateUser_ReturnsProblem_WhenCommandFails()
    {
        var command = new CreateUserCommand("jdoe", "jdoe@test.com", "John", "Doe");
        var error = new Error("Error.InternalServerError", "Something went wrong");

        _commandDispatcher
            .Dispatch<CreateUserCommand, Result<User>>(Arg.Any<CreateUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<User>.Failure(error));

        var result = await _sut.CreateUser(CreateRequestWithBody(command), CancellationToken.None);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }

    [Fact]
    public async Task CreateUser_ReturnsBadRequest_WhenBodyIsInvalidJson()
    {
        var result = await _sut.CreateUser(CreateRequestWithRawBody("{ not valid json"), CancellationToken.None);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, statusResult.StatusCode);
    }

    [Fact]
    public async Task CreateUser_ReturnsBadRequest_WhenBodyIsEmpty()
    {
        var result = await _sut.CreateUser(CreateEmptyRequest(), CancellationToken.None);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, statusResult.StatusCode);
    }

    [Fact]
    public async Task CreateUser_ReturnsValidationProblem_WhenValidationExceptionThrown()
    {
        var command = new CreateUserCommand("", "", "", "");
        var failures = new List<ValidationFailure>
        {
            new ValidationFailure("Username", "Username is required")
        };

        _commandDispatcher
            .Dispatch<CreateUserCommand, Result<User>>(Arg.Any<CreateUserCommand>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new ValidationException(failures));

        var result = await _sut.CreateUser(CreateRequestWithBody(command), CancellationToken.None);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, statusResult.StatusCode);
    }

    [Fact]
    public async Task CreateUser_ReturnsNotFound_WhenCommandReturnsNotFound()
    {
        var command = new CreateUserCommand("jdoe", "jdoe@test.com", "John", "Doe");

        _commandDispatcher
            .Dispatch<CreateUserCommand, Result<User>>(Arg.Any<CreateUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<User>.NotFound("User"));

        var result = await _sut.CreateUser(CreateRequestWithBody(command), CancellationToken.None);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, statusResult.StatusCode);
    }

    // ── UpdateUser ──────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateUser_ReturnsOk_WhenCommandSucceeds()
    {
        var command = new UpdateUserCommand(1, "jdoe", "jdoe@test.com", "John", "Doe");
        var user = new User { Id = 1, Username = "jdoe", Email = "jdoe@test.com", FirstName = "John", LastName = "Doe" };

        _commandDispatcher
            .Dispatch<UpdateUserCommand, Result<User>>(Arg.Any<UpdateUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<User>.Success(user));

        var result = await _sut.UpdateUser(CreateRequestWithBody(command), 1, CancellationToken.None);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status200OK, statusResult.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_ReturnsProblem_WhenCommandFails()
    {
        var command = new UpdateUserCommand(1, "jdoe", "jdoe@test.com", "John", "Doe");
        var error = new Error("Error.InternalServerError", "Something went wrong");

        _commandDispatcher
            .Dispatch<UpdateUserCommand, Result<User>>(Arg.Any<UpdateUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<User>.Failure(error));

        var result = await _sut.UpdateUser(CreateRequestWithBody(command), 1, CancellationToken.None);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_ReturnsBadRequest_WhenBodyIsInvalidJson()
    {
        var result = await _sut.UpdateUser(CreateRequestWithRawBody("{ not valid json"), 1, CancellationToken.None);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, statusResult.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_ReturnsBadRequest_WhenBodyIsEmpty()
    {
        var result = await _sut.UpdateUser(CreateEmptyRequest(), 1, CancellationToken.None);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, statusResult.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_ReturnsValidationProblem_WhenValidationExceptionThrown()
    {
        var command = new UpdateUserCommand(1, "", "", "", "");
        var failures = new List<ValidationFailure>
        {
            new ValidationFailure("Username", "Username is required")
        };

        _commandDispatcher
            .Dispatch<UpdateUserCommand, Result<User>>(Arg.Any<UpdateUserCommand>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new ValidationException(failures));

        var result = await _sut.UpdateUser(CreateRequestWithBody(command), 1, CancellationToken.None);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, statusResult.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_ReturnsNotFound_WhenCommandReturnsNotFound()
    {
        var command = new UpdateUserCommand(99, "jdoe", "jdoe@test.com", "John", "Doe");

        _commandDispatcher
            .Dispatch<UpdateUserCommand, Result<User>>(Arg.Any<UpdateUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<User>.NotFound("User"));

        var result = await _sut.UpdateUser(CreateRequestWithBody(command), 99, CancellationToken.None);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, statusResult.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_SetsIdFromRoute_WhenCommandBodyHasDifferentId()
    {
        var command = new UpdateUserCommand(0, "jdoe", "jdoe@test.com", "John", "Doe");
        var user = new User { Id = 5, Username = "jdoe", Email = "jdoe@test.com", FirstName = "John", LastName = "Doe" };

        UpdateUserCommand? dispatchedCommand = null;
        _commandDispatcher
            .Dispatch<UpdateUserCommand, Result<User>>(Arg.Do<UpdateUserCommand>(c => dispatchedCommand = c), Arg.Any<CancellationToken>())
            .Returns(Result<User>.Success(user));

        await _sut.UpdateUser(CreateRequestWithBody(command), 5, CancellationToken.None);

        Assert.NotNull(dispatchedCommand);
        Assert.Equal(5, dispatchedCommand!.Id);
    }

    // ── DeleteUser ──────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteUser_ReturnsNoContent_WhenCommandSucceeds()
    {
        _commandDispatcher
            .Dispatch<DeleteUserCommand, Result>(Arg.Any<DeleteUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _sut.DeleteUser(CreateEmptyRequest(), 1, CancellationToken.None);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, statusResult.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_ReturnsNotFound_WhenCommandReturnsNotFound()
    {
        _commandDispatcher
            .Dispatch<DeleteUserCommand, Result>(Arg.Any<DeleteUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.NotFound("User"));

        var result = await _sut.DeleteUser(CreateEmptyRequest(), 99, CancellationToken.None);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, statusResult.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_ReturnsProblem_WhenCommandFails()
    {
        var error = new Error("Error.InternalServerError", "Something went wrong");

        _commandDispatcher
            .Dispatch<DeleteUserCommand, Result>(Arg.Any<DeleteUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(error));

        var result = await _sut.DeleteUser(CreateEmptyRequest(), 1, CancellationToken.None);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_ReturnsValidationProblem_WhenValidationExceptionThrown()
    {
        var failures = new List<ValidationFailure>
        {
            new ValidationFailure("Id", "Id must be positive")
        };

        _commandDispatcher
            .Dispatch<DeleteUserCommand, Result>(Arg.Any<DeleteUserCommand>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new ValidationException(failures));

        var result = await _sut.DeleteUser(CreateEmptyRequest(), 0, CancellationToken.None);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, statusResult.StatusCode);
    }
}
