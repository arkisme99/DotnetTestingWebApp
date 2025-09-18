using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotnetTestingWebApp.Data;
using DotnetTestingWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetTestingWebApp.Services
{
    public partial class RoleService(ApplicationDbContext _context) : IRoleService
    {
        public IQueryable<ApplicationRole> GetAll()
        {
            return _context.ApplicationRoles.AsQueryable();
        }

        public async Task<ApplicationRole> GetByidAsync(string id)
        {
            var data = await _context.ApplicationRoles.FindAsync(id);
            return data!;
        }

        public async Task<Dictionary<string, List<Permission>>> GetPermissionsAsync()
        {
            // Ambil semua permission dulu ke memori
            var permissions = await _context.Permissions
                                            .ToListAsync();

            // Group berdasarkan kata terakhir dari CamelCase
            var grouped = permissions
                .GroupBy(p => SplitCamelCase(p.Name!).Last())
                .ToDictionary(g => g.Key, g => g.ToList());

            return grouped;
        }

        public async Task<List<Permission>> GetRoleWithPermissionsAsync(string roleId)
        {
            var rolePermissions = await _context.RolePermissions
                .Include(rp => rp.Permission)
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.Permission)
                .ToListAsync();

            return rolePermissions!;
        }

        public async Task<ApplicationRole> StoreAsync(ApplicationRole role, List<string> selectedPermissionNames)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                role.NormalizedName = role.Name!.ToUpperInvariant();
                _context.ApplicationRoles.Add(role);
                await _context.SaveChangesAsync();

                // Assign permissions berdasarkan Name
                if (selectedPermissionNames != null && selectedPermissionNames.Count != 0)
                {
                    // Console.WriteLine("Selected Permissions: " + string.Join(", ", selectedPermissionNames));
                    var permissions = _context.Permissions
                        .Where(p => selectedPermissionNames.Contains(p.Name))
                        .ToList();

                    foreach (var perm in permissions)
                    {
                        bool alreadyAssigned = _context.RolePermissions
                            .Any(rp => rp.RoleId == role.Id && rp.PermissionId == perm.Id);

                        if (!alreadyAssigned)
                        {
                            _context.RolePermissions.Add(new ApplicationRolePermission
                            {
                                RoleId = role.Id,
                                PermissionId = perm.Id
                            });
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                // Commit transaction
                await transaction.CommitAsync();

                return role;
            }
            catch
            {
                // Rollback jika ada error
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ApplicationRole> UpdateAsync(ApplicationRole role, List<string> selectedPermissionNames)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // role sudah di where dari controller

                // Update role data
                role.NormalizedName = role.Name!.ToUpperInvariant();
                _context.ApplicationRoles.Update(role);
                await _context.SaveChangesAsync();

                // Ambil permission lama
                var oldPermissionIds = await _context.RolePermissions
                    .Where(rp => rp.RoleId == role.Id)
                    .Select(rp => rp.PermissionId)
                    .ToListAsync();

                // Ambil permissionId dari selectedPermissionNames
                var newPermissionIds = await _context.Permissions
                    .Where(p => selectedPermissionNames.Contains(p.Name))
                    .Select(p => p.Id)
                    .ToListAsync();

                // Hitung diff
                var toAdd = newPermissionIds.Except(oldPermissionIds).ToList();
                var toRemove = oldPermissionIds.Except(newPermissionIds).ToList();

                // Remove yang tidak dipakai lagi
                if (toRemove.Count > 0)
                {
                    var removeEntities = _context.RolePermissions
                        .Where(rp => rp.RoleId == role.Id && toRemove.Contains(rp.PermissionId));
                    _context.RolePermissions.RemoveRange(removeEntities);
                }

                // Tambahkan yang baru
                if (toAdd.Count > 0)
                {
                    var addEntities = toAdd.Select(pid => new ApplicationRolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = pid
                    });
                    await _context.RolePermissions.AddRangeAsync(addEntities);
                }

                await _context.SaveChangesAsync();

                // Commit transaction
                await transaction.CommitAsync();

                return role;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // Fungsi split CamelCase
        static string[] SplitCamelCase(string input)
        {
            return [.. MyRegex().Matches(input)
                        .Cast<Match>()
                        .Select(m => m.Value)];
        }

        [GeneratedRegex(@"([A-Z][a-z0-9]+)")]
        private static partial Regex MyRegex();
    }
}