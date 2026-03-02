using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public class VersionFunction
{
    private readonly ILogger<VersionFunction> _logger;

    public VersionFunction(ILogger<VersionFunction> logger)
    {
        _logger = logger;
    }

    [Function("Version")]
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
