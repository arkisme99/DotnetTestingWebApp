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
        Task<Dictionary<string, List<Permission>>> GetPermissionsAsync();
        Task<ApplicationRole> StoreAsync(ApplicationRole role, List<string> selectedPermissionNames);
    }
}