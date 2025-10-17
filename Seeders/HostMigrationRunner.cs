using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetTestingWebApp.Data;
using Microsoft.EntityFrameworkCore;

namespace DotnetTestingWebApp.Seeders
{
    public static class HostMigrationRunner
    {
        public static async Task RunAsync(IServiceProvider serviceProvider)
        {
            var db = serviceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.MigrateAsync();
            Console.WriteLine("✅ Host database migration complete.");
        }

    }
}