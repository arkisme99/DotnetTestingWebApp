using System.Diagnostics;
using DotnetTestingWebApp.Extensions;
using DotnetTestingWebApp.Hubs;
using DotnetTestingWebApp.Seeders;
using DotnetTestingWebApp.Services;
using Finbuckle.MultiTenant;
using Hangfire;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// === Modular Configuration ===
builder.Services
    .AddDatabaseAndMultiTenant(config)
    .AddIdentityWithCookie()
    .AddHangfireWithMySql(config)
    .AddAuthorizationPolicies()
    .AddLocalizationSupport()
    .AddAppServices();

// Routing lowercase
builder.Services.AddSignalR();
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true; // optional
});

builder.Logging.AddConsole();
builder.WebHost.UseUrls("http://localhost:5000");

var app = builder.Build();

app.UseMultiTenant();

// panggil seeder di sini
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await TenantSeeder.SeedAsync(services);

    var tenantMigrator = services.GetRequiredService<TenantMigrationRunner>();
    await tenantMigrator.MigrateAllTenantsAsync();

    await IdentitySeeder.SeedAsync(services);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(locOptions.Value);

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire-panel");

app.MapHub<NotificationHub>("/hubs/notification");

// Custom Middleware: redirect kalau sudah login akses /Login
app.Use(async (context, next) =>
{
    // CEK LOGIN HOST
    if (context.User?.Identity?.IsAuthenticated == true)
    {
        if (context.Request.Path.StartsWithSegments("/auth/login") ||
            context.Request.Path.StartsWithSegments("/login") ||
            context.Request.Path.StartsWithSegments("/"))
        {
            context.Response.Redirect("/home/index");
            return;
        }
    }

    await next();

});


// Routing MVC (baru)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=auth}/{action=login}/{id?}");

app.Run();
