using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetTestingWebApp.Data;
using DotnetTestingWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetTestingWebApp.Seeders
{
    public static class TenantSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MultiTenantStoreDbContext>();
            await db.Database.MigrateAsync();

            if (!await db.Set<AppTenantInfo>().AnyAsync())
            {
                var tenants = new List<AppTenantInfo>
            {
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Identifier = "tenant1",
                    Name = "Tenant Satu",
                    ConnectionString = "server=localhost;port=3306;database=tenant1_db;user=root;password=;"
                },
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Identifier = "tenant2",
                    Name = "Tenant Dua",
                    ConnectionString = "server=localhost;port=3306;database=tenant2_db;user=root;password=;"
                }
            };

                db.AddRange(tenants);
                await db.SaveChangesAsync();
            }
        }

    }
}