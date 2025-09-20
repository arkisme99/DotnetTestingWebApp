using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DotnetTestingWebApp.Data;
using DotnetTestingWebApp.Models;
using DotnetTestingWebApp.Models.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DotnetTestingWebApp.Services
{
    public class UserService(ApplicationDbContext _context, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IActivityLogService _activityLogService, IHttpContextAccessor _httpContextAccessor) : IUserService
    {
        public IQueryable<UserListDto> GetAll()
        {
            // return _context.ApplicationUsers.AsQueryable();
            return _context.ApplicationUsers
                    .Select(u => new UserListDto
                    {
                        Id = u.Id,
                        FullName = u.FullName!,
                        UserName = u.UserName!,
                        Email = u.Email!,
                        Roles = (from ur in _context.UserRoles
                                 join r in _context.Roles on ur.RoleId equals r.Id
                                 where ur.UserId == u.Id
                                 select r.Name).ToList()
                    });

        }

        public IQueryable<UserListDto> GetAllDeleted()
        {
            // return _context.Products.IgnoreQueryFilters().Where(p => p.IsDeleted).AsQueryable();
            return _context.ApplicationUsers.IgnoreQueryFilters().Where(p => p.IsDeleted)
                    .Select(u => new UserListDto
                    {
                        Id = u.Id,
                        FullName = u.FullName!,
                        UserName = u.UserName!,
                        Email = u.Email!,
                        DeletedAt = u.DeletedAt.ToString()!,
                        Roles = (from ur in _context.UserRoles
                                 join r in _context.Roles on ur.RoleId equals r.Id
                                 where ur.UserId == u.Id
                                 select r.Name).ToList()
                    });
        }

        public async Task<ApplicationUser> GetByidAsync(string id)
        {
            var data = await _context.ApplicationUsers.FindAsync(id);
            return data!;
        }

        public async Task<List<SelectTwoDto>> GetRoleByidUserAsync(string id)
        {
            var user = await userManager.FindByIdAsync(id);

            var userRoles = await (from ur in _context.UserRoles
                                   join r in _context.Roles on ur.RoleId equals r.Id
                                   where ur.UserId == user!.Id
                                   select new SelectTwoDto
                                   {
                                       Id = r.Id,
                                       Text = r.Name!
                                   }).ToListAsync();

            return userRoles;
        }

        public async Task<ApplicationUser> StoreAsync(UserCreateDto dto)
        {

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // cek username unik
                var existingUser = await userManager.FindByNameAsync(dto.UserName);
                if (existingUser != null)
                    throw new Exception("Username already exists");

                // buat user baru
                var user = new ApplicationUser
                {
                    UserName = dto.UserName,
                    FullName = dto.Fullname,
                    Email = dto.Email ?? dto.UserName,
                    EmailConfirmed = true
                };

                var adminPassword = dto.Password;

                var result = await userManager.CreateAsync(user, adminPassword!);

                if (result.Succeeded)
                {
                    /* Console.WriteLine($"User berhasil dibuat:");
                    Console.WriteLine($"Id       : {user.Id}");
                    Console.WriteLine($"Username : {user.UserName}");
                    Console.WriteLine($"FullName : {user.FullName}");
                    Console.WriteLine($"Email    : {user.Email}"); */
                    //log manual
                    var objData = new
                    {
                        Id = user.Id,
                        Username = user.UserName,
                        FullName = user.FullName,
                        Email = user.Email
                    };
                    var userEntry = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
                    await _activityLogService.LogChangeAsync("ApplicationUser", "Added", userEntry, user.Id, objData);
                }

                if (dto.Roles != null && dto.Roles.Count > 0)
                {
                    foreach (var role in dto.Roles)
                    {
                        var roleEntity = await roleManager.FindByIdAsync(role) ?? throw new Exception($"Role with id {role} does not exist");
                        await userManager.AddToRoleAsync(user, roleEntity.Name!);
                    }
                }

                // Commit transaction
                await transaction.CommitAsync();

                return user;
            }
            catch
            {
                // Rollback jika ada error
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ApplicationUser> UpdateAsync(string userId, UserCreateDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var user = await userManager.FindByIdAsync(userId.ToString());
                if (user != null)
                {
                    var objDataOld = new
                    {
                        Id = user.Id,
                        Username = user.UserName,
                        FullName = user.FullName,
                        Email = user.Email
                    };
                    // update username & email
                    user.UserName = dto.UserName;
                    user.FullName = dto.Fullname;
                    user.Email = dto.Email ?? dto.UserName;

                    var updateResult = await userManager.UpdateAsync(user);

                    if (updateResult.Succeeded)
                    {
                        //log manual
                        var objDataNew = new
                        {
                            Id = user.Id,
                            Username = user.UserName,
                            FullName = user.FullName,
                            Email = user.Email
                        };

                        var objData = new
                        {
                            Original = objDataOld,
                            Current = objDataNew
                        };
                        var userEntry = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
                        await _activityLogService.LogChangeAsync("ApplicationUser", "Modified", userEntry, user.Id, objData);
                    }

                    // update password jika ada
                    if (!string.IsNullOrWhiteSpace(dto.Password))
                    {
                        var token = await userManager.GeneratePasswordResetTokenAsync(user);
                        var passResult = await userManager.ResetPasswordAsync(user, token, dto.Password);

                        if (!passResult.Succeeded)
                            throw new Exception(string.Join(", ", passResult.Errors.Select(e => e.Description)));
                    }

                    // sinkronisasi roles (clear + add ulang)
                    if (dto.Roles != null)
                    {
                        var currentRoles = await userManager.GetRolesAsync(user);
                        var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
                        if (!removeResult.Succeeded)
                            throw new Exception("Failed to clear old roles");

                        foreach (var role in dto.Roles)
                        {
                            var roleEntity = await roleManager.FindByIdAsync(role) ?? throw new Exception($"Role with id {role} does not exist");
                            await userManager.AddToRoleAsync(user, roleEntity.Name!);
                        }
                    }

                    await transaction.CommitAsync();
                    return user;
                }

                throw new Exception("User not found");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IdentityResult> SoftDeleteUserAsync(ApplicationUser user)
        {
            var objDataOld = new
            {
                Id = user.Id,
                Username = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                IsDeleted = user.IsDeleted,
                DeletedAt = user.DeletedAt
            };

            user.IsDeleted = true;
            user.DeletedAt = DateTime.Now;

            var objDataNew = new
            {
                Id = user.Id,
                Username = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                IsDeleted = true,
                DeletedAt = DateTime.Now
            };

            var objData = new
            {
                Original = objDataOld,
                Current = objDataNew
            };

            var userEntry = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
            await _activityLogService.LogChangeAsync("ApplicationUser", "SoftDeleted", userEntry, user.Id, objData);
            return await userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> SoftRestoreUserAsync(ApplicationUser user)
        {

            var objDataOld = new
            {
                Id = user.Id,
                Username = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                IsDeleted = user.IsDeleted,
                DeletedAt = user.DeletedAt
            };


            user.IsDeleted = false;
            user.DeletedAt = null;

            var objDataNew = new
            {
                Id = user.Id,
                Username = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                IsDeleted = false,
                DeletedAt = ""
            };

            var objData = new
            {
                Original = objDataOld,
                Current = objDataNew
            };

            var userEntry = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
            await _activityLogService.LogChangeAsync("ApplicationUser", "Modified", userEntry, user.Id, objData);

            return await userManager.UpdateAsync(user);
        }

        public async Task DeleteAsync(string id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await userManager.FindByIdAsync(id) ?? throw new Exception("User not found");

                // 1. Hapus semua role user, di komen karena hanya soft deleted
                /* var roles = await userManager.GetRolesAsync(user);
                if (roles.Any())
                {
                    var removeRoleResult = await userManager.RemoveFromRolesAsync(user, roles);
                    if (!removeRoleResult.Succeeded)
                    {
                        throw new Exception("Failed to clear roles on user");
                    }
                } */

                // 2. Hapus user
                // await userManager.DeleteAsync(user);
                await this.SoftDeleteUserAsync(user);
                // Commit transaction
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task RestoreAsync(string id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.ApplicationUsers.IgnoreQueryFilters().FirstAsync(u => u.Id == id) ?? throw new Exception("User not found");
                await this.SoftRestoreUserAsync(user);
                // Commit transaction
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<int> DeleteMultisAsync(string ids)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {

                if (string.IsNullOrWhiteSpace(ids))
                    return 0;

                var idList = ids.Split(',')
                                .Select(id => id.Trim())
                                .Where(id => !string.IsNullOrWhiteSpace(id))
                                .ToList();

                int deletedCount = 0;

                foreach (var id in idList)
                {
                    var user = await userManager.FindByIdAsync(id) ?? throw new Exception("User not found");

                    // 1. Hapus semua role user, dikkomen karena pakai softdeleted
                    /* var roles = await userManager.GetRolesAsync(user);
                    if (roles.Any())
                    {
                        var removeRoleResult = await userManager.RemoveFromRolesAsync(user, roles);
                        if (!removeRoleResult.Succeeded)
                        {
                            throw new Exception("Failed to clear roles on user");
                        }
                    } */

                    // 2. Hapus user
                    // await userManager.DeleteAsync(user);
                    await this.SoftDeleteUserAsync(user);
                    deletedCount++;
                }

                // Commit transaction
                await transaction.CommitAsync();
                return deletedCount;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

    }
}