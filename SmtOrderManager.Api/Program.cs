using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SmtOrderManager.Api.Middleware;
using SmtOrderManager.Infrastructure.DependencyInjection;
using SmtOrderManager.Infrastructure.Persistence;
using SmtOrderManager.Infrastructure.Seeding;



var builder = WebApplication.CreateBuilder(args);

// Share Data Protection keys with the Web UI so this API can validate the forwarded auth cookie.
// Use the same application name + key ring path in both projects.
var keyPathSetting = builder.Configuration["DataProtection:KeyPath"] ?? "../.keys";
var keyRingPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, keyPathSetting));
Directory.CreateDirectory(keyRingPath);

builder.Services
    .AddDataProtection()
    .SetApplicationName("SmtOrderManager")
    .PersistKeysToFileSystem(new DirectoryInfo(keyRingPath));

// Serilog configuration (reads from appsettings.json / appsettings.Development.json)
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Cookie authentication so the API can authenticate the same cookie issued by the Web UI.
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "SmtOrderManager.Auth";

        // APIs should not redirect to HTML pages.
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

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

//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//    db.Database.Migrate();
//}

if (app.Configuration.GetValue<bool>("RunMigrationsOnStartup"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment() && app.Configuration.GetValue<bool>("SeedData:Enabled"))
{
    await AppDbSeeder.SeedAsync(app.Services);
}



app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }

