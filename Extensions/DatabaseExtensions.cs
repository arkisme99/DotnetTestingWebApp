using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetTestingWebApp.Data;
using DotnetTestingWebApp.Models;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DotnetTestingWebApp.Extensions
{
    public static class DatabaseExtensions
    {
        public static IServiceCollection AddDatabaseAndMultiTenant(this IServiceCollection services, IConfiguration config)
        {
            // Host DB
            services.AddDbContext<MultiTenantStoreDbContext>(opt =>
                opt.UseMySql(
                    config.GetConnectionString("DefaultConnection"),
                    new MySqlServerVersion(new Version(8, 0, 36))
                ));

            // Multi-tenant
            services.AddMultiTenant<AppTenantInfo>()
                .WithEFCoreStore<MultiTenantStoreDbContext, AppTenantInfo>()
                .WithHostStrategy();

            // Tenant DB
            services.AddDbContext<ApplicationDbContext>((sp, opt) =>
            {
                var tenantContext = sp.GetRequiredService<IMultiTenantContextAccessor<AppTenantInfo>>().MultiTenantContext;
                var conn = tenantContext?.TenantInfo?.ConnectionString ?? config.GetConnectionString("DefaultConnection");
                opt.UseMySql(conn, new MySqlServerVersion(new Version(8, 0, 36)));
            });

            return services;
        }
    }
}