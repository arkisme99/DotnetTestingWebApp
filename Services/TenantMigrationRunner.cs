using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetTestingWebApp.Data;
using DotnetTestingWebApp.Models;
using DotnetTestingWebApp.Seeders;
using Microsoft.EntityFrameworkCore;

namespace DotnetTestingWebApp.Services
{
    public class TenantMigrationRunner(MultiTenantStoreDbContext storeContext, IServiceProvider serviceProvider)
    {
        private readonly MultiTenantStoreDbContext _storeContext = storeContext;
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public async Task MigrateAllTenantsAsync()
        {
            var tenants = await _storeContext.Set<AppTenantInfo>().ToListAsync();

            foreach (var tenant in tenants)
            {
                Console.WriteLine($"Migrating tenant: {tenant.Name} ({tenant.Identifier})");

                try
                {
                    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                    optionsBuilder.UseMySql(
                        tenant.ConnectionString,
                        new MySqlServerVersion(new Version(8, 0, 36)),
                        mySqlOptions => mySqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null)
                    );

                    var httpContextAccessor = _serviceProvider.GetRequiredService<IHttpContextAccessor>();
                    using var db = new ApplicationDbContext(optionsBuilder.Options, httpContextAccessor);

                    var canConnect = await db.Database.CanConnectAsync();
                    await db.Database.MigrateAsync();

                    Console.WriteLine(canConnect
                        ? $"✅ Migrated existing database for {tenant.Name}"
                        : $"✅ Created and migrated new database for {tenant.Name}");

                    // Jalankan seeder tenant
                    await TenantIdentitySeeder.SeedAsync(_serviceProvider, tenant.ConnectionString ?? "", tenant.Identifier ?? "tenant");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Migration failed for {tenant.Name}: {ex.Message}");
                }
            }
        }
    }
}