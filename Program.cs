using System.Diagnostics;
using System.Globalization;
using DotnetTestingWebApp.Authorization;
using DotnetTestingWebApp.Data;
using DotnetTestingWebApp.Hubs;
using DotnetTestingWebApp.Models;
using DotnetTestingWebApp.Seeders;
using DotnetTestingWebApp.Services;
using Hangfire;
using Hangfire.MySql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

//Service DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    );
    // options.AddInterceptors(new ActivityLogInterceptor(new HttpContextAccessor()));
});

// 🔹 Tambahkan Hangfire + MySQL Storage
builder.Services.AddHangfire(config =>
    config.UseStorage(
        new MySqlStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new MySqlStorageOptions
        {
            TablesPrefix = "Hangfire_", // prefix table
        })
    )
);

// Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        // Optional: konfigurasi password, lockout, dsb
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

//
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/auth/login";
    options.LogoutPath = "/auth/logout";
    options.AccessDeniedPath = "/home/index";

    // Tambahkan event handler
    options.Events = new Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationEvents
    {
        OnRedirectToLogin = context =>
        {
            // RedirectUri sudah mengandung ?ReturnUrl=...
            var redirectUri = context.RedirectUri;

            // tambahin pesan tanpa menghapus ReturnUrl
            var separator = redirectUri.Contains('?') ? "&" : "?";
            redirectUri += $"{separator}message=Harus+login+dulu+bro!";

            context.Response.Redirect(redirectUri);
            return Task.CompletedTask;
        },
        OnRedirectToAccessDenied = context =>
        {
            var redirectUri = context.RedirectUri;

            var separator = redirectUri.Contains('?') ? "&" : "?";
            redirectUri += $"{separator}message=Anda+Gak+punya+izin+bro!";

            context.Response.Redirect(redirectUri);
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddHangfireServer();
// Add services to the container.
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IActivityLogService, ActivityLogService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Routing lowercase
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true; // optional
});

builder.Logging.AddConsole();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Permission", policy =>
        policy.Requirements.Add(new PermissionRequirement("DYNAMIC")));

builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

// Tambahkan localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("en"),
        new CultureInfo("id")
    };

    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    // Gunakan cookie untuk menyimpan pilihan bahasa user
    options.RequestCultureProviders.Clear();
    options.RequestCultureProviders.Add(new CookieRequestCultureProvider());
    // options.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider());
});

builder.Services.AddSignalR();

builder.WebHost.UseUrls("http://localhost:5000");

var app = builder.Build();

// Gunakan cookie untuk simpan pilihan bahasa
/* localizationOptions.RequestCultureProviders.Insert(1, new QueryStringRequestCultureProvider());
localizationOptions.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider()); */

// 🔹 panggil seeder di sini
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await IdentitySeeder.SeedAsync(services);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHangfireDashboard("/hangfire-panel");

// app.UseRequestLocalization(localizationOptions);

app.UseHttpsRedirection();
app.UseStaticFiles();

var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(locOptions.Value);

app.UseRouting();
app.UseAuthentication();

app.MapHub<NotificationHub>("/hubs/notification");

// 🔹 Custom Middleware: redirect kalau sudah login akses /Login
app.Use(async (context, next) =>
{
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

app.UseAuthorization();

// Routing MVC (baru)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=auth}/{action=login}/{id?}");

app.Run();
