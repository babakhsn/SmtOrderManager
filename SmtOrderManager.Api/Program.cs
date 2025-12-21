using SmtOrderManager.Infrastructure.DependencyInjection;
using SmtOrderManager.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Phase 1: in-memory infrastructure so the API runs now
builder.Services.AddInfrastructure(builder.Configuration);

// Middleware registration
builder.Services.AddTransient<ExceptionHandlingMiddleware>();

var app = builder.Build();


// Exception handling should be early in the pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Basic request logging (later replaced/enhanced with Serilog request logging)
app.Use(async (ctx, next) =>
{
    var logger = ctx.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("RequestLogging");
    logger.LogInformation("HTTP {Method} {Path}", ctx.Request.Method, ctx.Request.Path);
    await next();
    logger.LogInformation("HTTP {Method} {Path} -> {StatusCode}", ctx.Request.Method, ctx.Request.Path, ctx.Response.StatusCode);
});


app.MapGet("/test", () => Results.Ok(new { status = "ok" }));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
