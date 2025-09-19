using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DotnetTestingWebApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DotnetTestingWebApp.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext<ApplicationUser, ApplicationRole, string>(options)
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<ApplicationRole> ApplicationRoles { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<ApplicationRolePermission> RolePermissions { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }

        /* public override int SaveChanges()
        {
            // ApplyAuditInfo();
            Console.WriteLine("CekInterceptors_HarusnyaSesudah");
            ApplySoftDeleted();
            return base.SaveChanges();
        } */

        /* public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
        {
            // ApplyAuditInfo();
            Console.WriteLine("CekInterceptors_HarusnyaSesudah");
            ApplySoftDeleted();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        } */

        private void ApplySoftDeleted()
        {
            foreach (var entry in ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Deleted))
            {
                // ubah dari "Delete" ke "Update" dengan IsDeleted = true
                entry.State = EntityState.Modified;
                entry.CurrentValues["IsDeleted"] = true;
                entry.CurrentValues["DeletedAt"] = DateTime.UtcNow;
            }
        }

        private void ApplyAuditInfo()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is AuditableEntity &&
                            (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                var entity = (AuditableEntity)entityEntry.Entity;

                if (entityEntry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.Now;
                }
                else
                {
                    entityEntry.Property(nameof(AuditableEntity.CreatedAt)).IsModified = false;
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

            // setup manual soft deleted
            // builder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);

            //dinamis sotf deleted model
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                // cek apakah entity punya properti "IsDeleted"
                var prop = entityType.ClrType.GetProperty("IsDeleted");
                if (prop != null && prop.PropertyType == typeof(bool))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var property = Expression.Property(parameter, "IsDeleted");
                    var filter = Expression.Lambda(
                        Expression.Equal(property, Expression.Constant(false)),
                        parameter
                    );

                    builder.Entity(entityType.ClrType).HasQueryFilter(filter);
                }
            }
        }
    }
}