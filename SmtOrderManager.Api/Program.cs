using SmtOrderManager.Infrastructure.DependencyInjection;
using SmtOrderManager.Api.Middleware;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

// Serilog configuration (reads from appsettings.json / appsettings.Development.json)
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Infrastructure (EF + SQLite)
builder.Services.AddInfrastructure(builder.Configuration);

// Middleware registration
builder.Services.AddTransient<ExceptionHandlingMiddleware>();

var app = builder.Build();


// Exception handling should be early in the pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Serilog request logging (replaces the custom inline request logging middleware)
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
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
