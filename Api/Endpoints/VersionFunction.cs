using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using System.Net;

namespace Api.Endpoints;

public class VersionFunction
{
    private readonly ILogger<VersionFunction> _logger;

    public VersionFunction(ILogger<VersionFunction> logger)
    {
        _logger = logger;
    }

    [Function("Version")]
    [OpenApiOperation(operationId: "GetVersion", tags: ["Version"], Summary = "Get API version", Description = "Returns the current API version information.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(object), Summary = "Version information", Description = "The current version information as JSON.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Summary = "Not found", Description = "The version.json file was not found.")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "version")] HttpRequest req)
    {
        _logger.LogInformation("Version function processed a request.");

        var assemblyDir = AppContext.BaseDirectory;
        var versionFilePath = Path.Combine(assemblyDir, "version.json");

        if (!File.Exists(versionFilePath))
        {
            _logger.LogWarning("version.json not found at {Path}", versionFilePath);
            return new NotFoundObjectResult("version.json not found.");
        }

        var json = File.ReadAllText(versionFilePath);
        return new ContentResult
        {
            Content = json,
            ContentType = "application/json",
            StatusCode = 200
        };
    }
}
