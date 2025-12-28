using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using SmtOrderManager.Api.Middleware;
using SmtOrderManager.Domain.Common;
using System.Text.Json;
using Xunit;

namespace SmtOrderManager.IntegrationTests.Api;

public sealed class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task Middleware_WhenNotFoundException_Returns404ProblemJson()
    {
        // Arrange
        var env = new FakeHostEnvironment(isDevelopment: true);
        var middleware = new ExceptionHandlingMiddleware(
            NullLogger<ExceptionHandlingMiddleware>.Instance,
            env
        );

        var ctx = new DefaultHttpContext();
        ctx.Request.Path = "/api/orders/some-id";
        ctx.Response.Body = new MemoryStream();

        RequestDelegate next = _ => throw new NotFoundException("Order not found.");

        // Act
        await middleware.InvokeAsync(ctx, next);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, ctx.Response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", ctx.Response.ContentType);
        // "application/problem+json"

        ctx.Response.Body.Position = 0;
        using var doc = await JsonDocument.ParseAsync(ctx.Response.Body);
        var root = doc.RootElement;

        Assert.Equal(404, root.GetProperty("status").GetInt32());
        Assert.Equal("Resource not found", root.GetProperty("title").GetString());
        Assert.True(!root.TryGetProperty("extensions", out var ext));
        Assert.True(ext.TryGetProperty("traceId", out _));
    }

    private sealed class FakeHostEnvironment : IHostEnvironment
    {
        public FakeHostEnvironment(bool isDevelopment) => IsDevelopmentFlag = isDevelopment;

        private bool IsDevelopmentFlag { get; }

        public string EnvironmentName { get; set; } = Environments.Production;
        public string ApplicationName { get; set; } = "TestApp";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IServiceProvider? ServiceProvider { get; set; }

        public IFileProvider ContentRootFileProvider { get; set; } =
            new Microsoft.Extensions.FileProviders.NullFileProvider();

        public bool IsDevelopment() => IsDevelopmentFlag;
    }
}
