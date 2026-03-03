using Api.Endpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Api.Tests;

public class VersionFunctionTests
{
    private readonly ILogger<VersionFunction> _logger;
    private readonly VersionFunction _sut;

    public VersionFunctionTests()
    {
        _logger = Substitute.For<ILogger<VersionFunction>>();
        _sut = new VersionFunction(_logger);
    }

    private static HttpRequest CreateRequest()
    {
        var context = new DefaultHttpContext();
        return context.Request;
    }

    [Fact]
    public void Run_ReturnsOkWithJsonContent_WhenVersionFileExists()
    {
        // version.json is copied to AppContext.BaseDirectory via the .csproj CopyToOutputDirectory setting
        var versionFilePath = Path.Combine(AppContext.BaseDirectory, "version.json");
        Assert.True(File.Exists(versionFilePath), $"version.json must exist at {versionFilePath} for this test to run.");

        var result = _sut.Run(CreateRequest());

        var contentResult = Assert.IsType<ContentResult>(result);
        Assert.Equal(200, contentResult.StatusCode);
        Assert.Equal("application/json", contentResult.ContentType);
        Assert.NotNull(contentResult.Content);
        Assert.NotEmpty(contentResult.Content!);
    }

    [Fact]
    public void Run_ReturnsNotFound_WhenVersionFileDoesNotExist()
    {
        // Temporarily rename the version.json if it exists so the function can't find it
        var versionFilePath = Path.Combine(AppContext.BaseDirectory, "version.json");
        var tempPath = versionFilePath + ".bak";
        bool fileExisted = File.Exists(versionFilePath);
        if (fileExisted)
        {
            File.Move(versionFilePath, tempPath);
        }

        try
        {
            var result = _sut.Run(CreateRequest());

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }
        finally
        {
            if (fileExisted)
            {
                File.Move(tempPath, versionFilePath);
            }
        }
    }
}
