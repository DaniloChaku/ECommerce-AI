using System.Net;
using System.Text.Json;
using ECommerce.API.Middleware;
using ECommerce.DAL.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Logging;

namespace ECommerce.UnitTests.API.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    private readonly FakeLogger<ExceptionHandlingMiddleware> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ExceptionHandlingMiddlewareTests()
    {
        _logger = new FakeLogger<ExceptionHandlingMiddleware>();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task InvokeAsync_WhenNoException_CallsNext()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var wasCalled = false;

        RequestDelegate next = (ctx) =>
        {
            wasCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new ExceptionHandlingMiddleware(next, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(wasCalled);
        Assert.Empty(_logger.Collector.GetSnapshot());
    }

    [Fact]
    public async Task InvokeAsync_WhenBaseApiException_ReturnsExpectedResponse()
    {
        // Arrange
        var context = GetHttpContextWithResponseBody();
        var exception = new InvalidCredentialsException();

        var middleware = new ExceptionHandlingMiddleware(ctx => throw exception, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var result = await DeserializeResponseAsync(context.Response.Body);

        Assert.Equal((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
        Assert.Equal("Invalid email or password", result!.Message);
        Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        Assert.Empty(_logger.Collector.GetSnapshot());
    }

    [Fact]
    public async Task InvokeAsync_WhenUnhandledException_ReturnsInternalServerErrorAndLogsError()
    {
        // Arrange
        var context = GetHttpContextWithResponseBody();
        var exception = new Exception("Some unexpected error");

        var middleware = new ExceptionHandlingMiddleware(ctx => throw exception, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var result = await DeserializeResponseAsync(context.Response.Body);

        Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
        Assert.Equal("An unexpected error occurred", result!.Message);
        Assert.Equal((int)HttpStatusCode.InternalServerError, result.StatusCode);

        var logs = _logger.Collector.GetSnapshot();
        Assert.Contains(logs, log => log.Level == LogLevel.Error && log.Message.Contains("Unhandled exception"));
    }

    private static DefaultHttpContext GetHttpContextWithResponseBody()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private async Task<ErrorResponse?> DeserializeResponseAsync(Stream responseBody)
    {
        responseBody.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(responseBody);
        var responseText = await reader.ReadToEndAsync();
        return JsonSerializer.Deserialize<ErrorResponse>(responseText, _jsonOptions);
    }
}
