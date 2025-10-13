using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.MySql;

namespace DotnetTestingWebApp.Extensions
{
    public static class HangfireExtensions
    {
        public static IServiceCollection AddHangfireWithMySql(this IServiceCollection services, IConfiguration config)
        {
            services.AddHangfire(cfg =>
                cfg.UseStorage(new MySqlStorage(
                    config.GetConnectionString("DefaultConnection"),
                    new MySqlStorageOptions { TablesPrefix = "Hangfire_" }
                )));
            services.AddHangfireServer();
            return services;
        }
    }
}