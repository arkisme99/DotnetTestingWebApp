using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DotnetTestingWebApp.Data;
using DotnetTestingWebApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DotnetTestingWebApp.Services
{
    public class AuthService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager, ILogger<AuthService> logger, IActivityLogService _activityLogService, IHttpContextAccessor _httpContextAccessor) : IAuthService
    {
        /* private readonly ApplicationDbContext _context = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signManager;
        private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
        private readonly ILogger<AuthService> _logger = logger; */


        public async Task<bool> LoginAsync(string email, string password)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null) return false;

            // 🔹 cek password
            var result = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
            if (!result.Succeeded) return false;

            // 🔹 ambil role user
            var roles = await userManager.GetRolesAsync(user);

            // 🔹 ambil permission dari role
            var permissions = await context.RolePermissions
                .Where(rp => roles.Contains(rp.Role!.Name!))
                .Select(rp => rp.Permission!.Name)
                .ToListAsync();

            // 🔹 build claims
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.UserName ?? user.Email ?? email),
                new(ClaimTypes.Name, user.FullName ?? "Nama Lengkap")
            };

            // role → claim
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            // permission → claim
            claims.AddRange(permissions.Select(p => new Claim("Permission", p)));

            // 🔹 buat identity baru
            var claimsIdentity = new ClaimsIdentity(claims, "Identity.Application");

            // 🔹 sign in ulang dengan claims
            await signInManager.SignOutAsync(); // clear dulu biar gak duplikat
            await signInManager.Context.SignInAsync(
                "Identity.Application",
                new ClaimsPrincipal(claimsIdentity)
            );

            logger.LogInformation("Proses Login {email}, {roles}, {permissions}", email, roles, permissions);
            await _activityLogService.LogChangeAsync(null, "Login", user.Id, null, null);

            return true;
        }

        public async Task LogoutAsync()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
            await _activityLogService.LogChangeAsync(null, "Logout", userId, null, null);
            await signInManager.SignOutAsync();
        }

        public async Task<bool> RegisterAsync(string email, string password)
        {
            var user = new ApplicationUser { UserName = email, Email = email };
            var result = await userManager.CreateAsync(user, password);
            return result.Succeeded;
        }

        public async Task<bool> AssignRoleAsync(string email, string roleName)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null) return false;

            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new ApplicationRole { Name = roleName });

            var result = await userManager.AddToRoleAsync(user, roleName);
            return result.Succeeded;
        }


    }
}