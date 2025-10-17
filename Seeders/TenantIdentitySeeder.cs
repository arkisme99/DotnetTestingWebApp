using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetTestingWebApp.Data;
using DotnetTestingWebApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DotnetTestingWebApp.Seeders
{
    public static class TenantIdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider, string connectionString, string tenantIdentifier)
        {
            // 1️⃣ Siapkan DbContext khusus untuk tenant
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 0, 36))
            );

            var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            using var dbContext = new ApplicationDbContext(optionsBuilder.Options, httpContextAccessor);

            // Pastikan DB bisa diakses
            if (!await dbContext.Database.CanConnectAsync())
            {
                Console.WriteLine("❌ Cannot connect to tenant database.");
                return;
            }

            //
            var userStore = new UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, string>(dbContext);
            var roleStore = new RoleStore<ApplicationRole, ApplicationDbContext, string>(dbContext);

            var userManager = ActivatorUtilities.CreateInstance<UserManager<ApplicationUser>>(serviceProvider, userStore);
            var roleManager = ActivatorUtilities.CreateInstance<RoleManager<ApplicationRole>>(serviceProvider, roleStore);

            // 3️⃣ Seed Roles
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

            // 4️⃣ Seed Permissions
            var permissions = new List<Permission>
            {
                new() { Name = "ViewUser" },
                new() { Name = "CreateUser" },
                new() { Name = "EditUser" },
                new() { Name = "DeleteUser" },
                new() { Name = "MultiDeleteUser" },
                new() { Name = "ViewRole" },
                new() { Name = "CreateRole" },
                new() { Name = "EditRole" },
                new() { Name = "DeleteRole" },
                new() { Name = "MultiDeleteRole" },
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

            // 5️⃣ Assign Permissions to Administrator role
            var adminRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Administrator");
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

            // 6️⃣ Seed Default Admin User
            string adminEmail = $"admin@{tenantIdentifier}.example.com";
            string fullName = "Tenant Administrator";
            string adminPassword = "AdminTenant123!";

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var user = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = fullName,
                };

                var result = await userManager.CreateAsync(user, adminPassword);
                if (result.Succeeded && adminRole != null)
                {
                    await userManager.AddToRoleAsync(user, adminRole.Name!);
                }
            }

            Console.WriteLine($"✅ Tenant seeding complete for {tenantIdentifier}");
        }
    }
}