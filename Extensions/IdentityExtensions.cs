using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetTestingWebApp.Data;
using DotnetTestingWebApp.Models;
using Microsoft.AspNetCore.Identity;

namespace DotnetTestingWebApp.Extensions
{
    public static class IdentityExtensions
    {
        public static IServiceCollection AddIdentityWithCookie(this IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/auth/login";
                options.LogoutPath = "/auth/logout";
                options.AccessDeniedPath = "/home/index";

                options.Events = new Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationEvents
                {
                    OnRedirectToLogin = ctx =>
                    {
                        var redirect = $"{ctx.RedirectUri}{(ctx.RedirectUri.Contains('?') ? "&" : "?")}message=Harus+login+dulu+bro!";
                        ctx.Response.Redirect(redirect);
                        return Task.CompletedTask;
                    },
                    OnRedirectToAccessDenied = ctx =>
                    {
                        var redirect = $"{ctx.RedirectUri}{(ctx.RedirectUri.Contains('?') ? "&" : "?")}message=Anda+Gak+punya+izin+bro!";
                        ctx.Response.Redirect(redirect);
                        return Task.CompletedTask;
                    }
                };
            });

            return services;
        }

    }
}