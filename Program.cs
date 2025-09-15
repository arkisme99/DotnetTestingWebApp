using DotnetTestingWebApp.Data;
using DotnetTestingWebApp.Models;
using DotnetTestingWebApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//Service DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseMySql(
    builder.Configuration.GetConnectionString("DefaultConnection"),
    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
));

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
    options.AccessDeniedPath = "/auth/AccessDenied";

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
        }
    };
});

//MVC
builder.Services.AddControllersWithViews();

// Add services to the container.
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Tambahkan service sebelum Build()
builder.Services.AddControllersWithViews();

// Routing lowercase
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true; // optional
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Routing MVC (baru)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=auth}/{action=login}/{id?}");

app.Run();
