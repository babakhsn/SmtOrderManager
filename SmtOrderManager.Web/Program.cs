using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using SmtOrderManager.Web.Auth;

var builder = WebApplication.CreateBuilder(args);

// Share Data Protection keys with the API so the forwarded auth cookie can be validated there.
// Use the same application name + key ring path in both projects.
var keyPathSetting = builder.Configuration["DataProtection:KeyPath"] ?? "../.keys";
var keyRingPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, keyPathSetting));
Directory.CreateDirectory(keyRingPath);

builder.Services
    .AddDataProtection()
    .SetApplicationName("SmtOrderManager")
    .PersistKeysToFileSystem(new DirectoryInfo(keyRingPath));

builder.Services.AddRazorPages();
builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection("Auth"));

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<ApiCookieForwardingHandler>();

builder.Services.AddHttpClient("Api", (sp, client) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var baseUrl = cfg["Api:BaseUrl"] ?? throw new InvalidOperationException("Api:BaseUrl is missing.");
    client.BaseAddress = new Uri(baseUrl);
})
.AddHttpMessageHandler<ApiCookieForwardingHandler>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "SmtOrderManager.Auth";
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.Run();
