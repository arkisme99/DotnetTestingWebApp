using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetTestingWebApp.Data;
using DotnetTestingWebApp.Models;
using DotnetTestingWebApp.Models.Dto;
using Microsoft.AspNetCore.Identity;

namespace DotnetTestingWebApp.Services
{
    public class UserService(ApplicationDbContext _context, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager) : IUserService
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

        public async Task<ApplicationUser> GetByidAsync(string id)
        {
            var data = await _context.ApplicationUsers.FindAsync(id);
            return data!;
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

                Console.WriteLine($"CekHasil : {result} , User : {user}");

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
                    // update username & email
                    user.UserName = dto.UserName;
                    user.FullName = dto.Fullname;
                    user.Email = dto.Email ?? dto.UserName;

                    var updateResult = await userManager.UpdateAsync(user);

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
                            if (!await roleManager.RoleExistsAsync(role))
                                throw new Exception($"Role {role} does not exist");

                            var addResult = await userManager.AddToRoleAsync(user, role);
                            if (!addResult.Succeeded)
                                throw new Exception($"Failed to add role {role}");
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

    }
}