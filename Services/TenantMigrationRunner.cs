using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetTestingWebApp.Data;
using DotnetTestingWebApp.Models;
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

                    // Pastikan database ada
                    if (!await db.Database.CanConnectAsync())
                    {
                        Console.WriteLine($"Database for {tenant.Identifier} not found. Creating...");
                        await db.Database.EnsureCreatedAsync();
                    }


                    await db.Database.EnsureCreatedAsync();
                    await db.Database.MigrateAsync();

                    Console.WriteLine($"✅ Migration complete for {tenant.Name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Migration failed for {tenant.Name}: {ex.Message}");
                }
            }
        }
    }
}