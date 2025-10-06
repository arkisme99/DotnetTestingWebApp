using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DotnetTestingWebApp.Data;
using DotnetTestingWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DotnetTestingWebApp.Controllers
{
    public class TenantsController : Controller
    {
        private readonly MultiTenantStoreDbContext _storeDbContext;

        public TenantsController(MultiTenantStoreDbContext storeDbContext)
        {
            _storeDbContext = storeDbContext;
        }

        public async Task<IActionResult> Index()
        {
            var tenants = await _storeDbContext.TenantInfo.ToListAsync();
            return View(tenants);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(AppTenantInfo tenant)
        {
            if (!ModelState.IsValid)
                return View(tenant);

            tenant.Id = Guid.NewGuid().ToString();

            // Set ConnectionString tenant
            tenant.ConnectionString = $"Server=localhost;Database={tenant.Identifier}DB;User=root;Password=;";

            _storeDbContext.Add(tenant);
            await _storeDbContext.SaveChangesAsync();

            // Buat database tenant
            await CreateTenantDatabaseAsync(tenant);

            return RedirectToAction(nameof(Index));
        }

        private static async Task CreateTenantDatabaseAsync(AppTenantInfo tenant)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseMySql(
                tenant.ConnectionString,
                new MySqlServerVersion(new Version(8, 0, 36))
            );

            using var db = new ApplicationDbContext(optionsBuilder.Options, null!);
            await db.Database.MigrateAsync();
        }
    }

}