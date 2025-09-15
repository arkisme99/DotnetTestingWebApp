using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetTestingWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetTestingWebApp.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<ApplicationRolePermission> RolePermissions { get; set; }

        public DbSet<ApplicationUser> Users { get; set; }

        public override int SaveChanges()
        {
            ApplyAuditInfo();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
        {
            ApplyAuditInfo();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void ApplyAuditInfo()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is Product &&
                            (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                var entity = (Product)entityEntry.Entity;

                if (entityEntry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.Now;
                }
                else
                {
                    // Pastikan CreatedAt tidak ikut diubah saat update
                    entityEntry.Property(nameof(Product.CreatedAt)).IsModified = false;
                }

                entity.UpdatedAt = DateTime.Now;
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationRolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            builder.Entity<ApplicationRolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany() // atau .WithMany(r => r.RolePermissions) kalau kamu tambahkan koleksi di ApplicationRole
                .HasForeignKey(rp => rp.RoleId);

            builder.Entity<ApplicationRolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);
        }
    }
}