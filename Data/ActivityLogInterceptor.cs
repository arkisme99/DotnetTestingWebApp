using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using DotnetTestingWebApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DotnetTestingWebApp.Data
{
    public class ActivityLogInterceptor(IHttpContextAccessor _httpContextAccessor) : SaveChangesInterceptor
    {

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            LogChanges(eventData.Context!);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            LogChanges(eventData.Context!);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void LogChanges(DbContext context)
        {
            if (context == null) return;

            var user = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "System";

            foreach (var entry in context.ChangeTracker.Entries()
                     .Where(e => e.State == EntityState.Added
                              || e.State == EntityState.Modified
                              || e.State == EntityState.Deleted))
            {
                //Skip ActivityLog
                if (entry.Entity is ActivityLog) continue;

                var changes = entry.State switch
                {
                    EntityState.Added => entry.CurrentValues.ToObject(),
                    EntityState.Modified => entry.Properties
                        .Where(p => p.IsModified && p.Metadata.Name != nameof(AuditableEntity.UpdatedAt))
                        .ToDictionary(p => p.Metadata.Name, p => new { Original = p.OriginalValue, Current = p.CurrentValue }),
                    EntityState.Deleted => entry.CurrentValues.ToObject(),
                    _ => null
                };

                // var entity = (AuditableEntity)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    entry.Property(nameof(AuditableEntity.CreatedAt)).CurrentValue = DateTime.Now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Property(nameof(AuditableEntity.CreatedAt)).IsModified = false;
                    entry.Property(nameof(AuditableEntity.UpdatedAt)).CurrentValue = DateTime.Now;
                }
                else if (entry.State == EntityState.Deleted)
                {
                    // entry.State = EntityState.Modified;
                    // entity.IsDeleted = true;
                    // entity.DeletedAt = DateTime.Now;
                    entry.Property(nameof(AuditableEntity.IsDeleted)).CurrentValue = true;
                    entry.Property(nameof(AuditableEntity.DeletedAt)).CurrentValue = DateTime.Now;
                }


                context.Set<ActivityLog>().Add(new ActivityLog
                {
                    EntityName = entry.Entity.GetType().Name,
                    Action = entry.State.ToString(),
                    ChangedBy = user,
                    EntityId = GetPrimaryKey(entry),
                    Changes = JsonSerializer.Serialize(changes),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
            }
        }


        private static string GetPrimaryKey(EntityEntry entry)
        {
            ArgumentNullException.ThrowIfNull(entry);
            var key = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
            return key?.CurrentValue?.ToString() ?? "";
        }

        private static string GetChanges(EntityEntry entry)
        {
            if (entry.State == EntityState.Modified)
            {
                var changes = entry.Properties
                    .Where(p => p.IsModified)
                    .ToDictionary(p => p.Metadata.Name, p => new { Original = p.OriginalValue, Current = p.CurrentValue });
                return JsonSerializer.Serialize(changes);
            }
            else if (entry.State == EntityState.Added)
            {
                return JsonSerializer.Serialize(entry.CurrentValues.ToObject());
            }
            else if (entry.State == EntityState.Deleted)
            {
                return JsonSerializer.Serialize(entry.OriginalValues.ToObject());
            }
            return null!;
        }

    }
}