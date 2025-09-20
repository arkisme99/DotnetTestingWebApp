using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DotnetTestingWebApp.Data;
using DotnetTestingWebApp.Models;

namespace DotnetTestingWebApp.Services
{
    public class ActivityLogService(ApplicationDbContext _context) : IActivityLogService
    {
        public async Task LogChangeAsync(
        string? entityName,
        string stringAction,
        string user,
        string? entityId,
        object? changes)
        {
            var log = new ActivityLog
            {
                EntityName = entityName,
                Action = stringAction,
                ChangedBy = user,
                EntityId = entityId,
                Changes = JsonSerializer.Serialize(changes),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }

    }
}