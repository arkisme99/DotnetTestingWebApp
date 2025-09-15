using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DotnetTestingWebApp.Data;
using DotnetTestingWebApp.Models;
using Microsoft.AspNetCore.Identity;

namespace DotnetTestingWebApp.Middlewares
{
    public class PermissionMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager, ApplicationDbContext db)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint == null)
            {
                await _next(context);
                return;
            }

            var permissionRequired = endpoint.Metadata.GetMetadata<PermissionAttribute>();
            if (permissionRequired == null)
            {
                await _next(context);
                return;
            }

            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                context.Response.Redirect("/auth/login");
                return;
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                context.Response.Redirect("/auth/login");
                return;
            }

            var userRoles = await userManager.GetRolesAsync(user);
            var rolePermissions = db.RolePermissions
                .Where(rp => userRoles.Contains(rp.Role!.Name!))
                .Select(rp => rp.Permission!.Name)
                .ToList();

            if (!rolePermissions.Contains(permissionRequired.Name))
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Forbidden");
                return;
            }

            await _next(context);
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class PermissionAttribute(string name) : Attribute
    {
        public string Name { get; } = name;
    }
}