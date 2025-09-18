using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetTestingWebApp.Models;

namespace DotnetTestingWebApp.Services
{
    public interface IRoleService
    {
        IQueryable<ApplicationRole> GetAll();
        Task<ApplicationRole> GetByidAsync(string id);
        Task<Dictionary<string, List<Permission>>> GetPermissionsAsync();
        Task<List<Permission>> GetRoleWithPermissionsAsync(string roleId);
        Task<ApplicationRole> UpdateAsync(ApplicationRole role, List<string> selectedPermissionNames);
        Task<ApplicationRole> StoreAsync(ApplicationRole role, List<string> selectedPermissionNames);
        Task DeleteAsync(string id);
        Task<int> DeleteMultisAsync(string ids);
    }
}