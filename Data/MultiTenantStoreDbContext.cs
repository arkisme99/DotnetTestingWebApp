using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetTestingWebApp.Models;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.EntityFrameworkCore.Stores.EFCoreStore;
using Microsoft.EntityFrameworkCore;

namespace DotnetTestingWebApp.Data
{
    public class MultiTenantStoreDbContext : EFCoreStoreDbContext<AppTenantInfo>
    {
        public MultiTenantStoreDbContext(DbContextOptions<MultiTenantStoreDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppTenantInfo>().ToTable("Tenants");
        }

    }
}