using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetTestingWebApp.Data;
using DotnetTestingWebApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DotnetTestingWebApp.Seeders
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // 1. Seed Roles
            var roles = new List<ApplicationRole>
            {
                new() { Name = "Administrator", NormalizedName = "ADMININSTRATOR", Description = "Administrator role" },
                new() { Name = "User", NormalizedName = "USER", Description = "User role" }
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role.Name!))
                {
                    await roleManager.CreateAsync(role);
                }
            }

            // 2. Seed Permissions
            var permissions = new List<Permission>
            {
                new() { Name = "ViewUser" },
                new() { Name = "CreateUser" },
                new() { Name = "EditUser" },
                new() { Name = "DeleteUser" },
                new() { Name = "MultiDeleteUser" },
                new() { Name = "ViewProduct" },
                new() { Name = "CreateProduct" },
                new() { Name = "EditProduct" },
                new() { Name = "DeleteProduct" },
                new() { Name = "MultiDeleteProduct" }
            };

            foreach (var permission in permissions)
            {
                if (!dbContext.Permissions.Any(p => p.Name == permission.Name))
                {
                    dbContext.Permissions.Add(permission);
                }
            }
            await dbContext.SaveChangesAsync();

            // 3. Assign Permissions to Administrator role
            var adminRole = await roleManager.FindByNameAsync("Administrator");
            if (adminRole != null)
            {
                var allPermissions = dbContext.Permissions.ToList();

                foreach (var perm in allPermissions)
                {
                    bool alreadyAssigned = dbContext.RolePermissions
                        .Any(rp => rp.RoleId == adminRole.Id && rp.PermissionId == perm.Id);

                    if (!alreadyAssigned)
                    {
                        dbContext.RolePermissions.Add(new ApplicationRolePermission
                        {
                            RoleId = adminRole.Id,
                            PermissionId = perm.Id
                        });
                    }
                }
                await dbContext.SaveChangesAsync();
            }

            // 4. Seed Default Admin User
            string adminEmail = "admin@example.com";
            string adminPassword = "Admin123!"; // hashed otomatis

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var user = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, adminPassword);

                if (result.Succeeded && adminRole != null)
                {
                    await userManager.AddToRoleAsync(user, adminRole.Name!);
                }
            }

            // 5. Assign Permissions to User role
            var userRole = await roleManager.FindByNameAsync("User");
            if (userRole != null)
            {
                var viewUserPermission = await dbContext.Permissions.FirstOrDefaultAsync(p => p.Name == "ViewProduct");

                if (viewUserPermission != null)
                {
                    bool alreadyAssigned = dbContext.RolePermissions
                        .Any(rp => rp.RoleId == userRole.Id && rp.PermissionId == viewUserPermission.Id);

                    if (!alreadyAssigned)
                    {
                        dbContext.RolePermissions.Add(new ApplicationRolePermission
                        {
                            RoleId = userRole.Id,
                            PermissionId = viewUserPermission.Id
                        });

                        await dbContext.SaveChangesAsync();
                    }
                }
            }

            // 6. Seed Default Admin User
            string userEmail = "user@example.com";
            string userPassword = "User123!"; // hashed otomatis

            if (await userManager.FindByEmailAsync(userEmail) == null)
            {
                var user = new ApplicationUser
                {
                    UserName = userEmail,
                    Email = userEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, userPassword);

                if (result.Succeeded && userRole != null)
                {
                    await userManager.AddToRoleAsync(user, userRole.Name!);
                }
            }
        }
    }
}