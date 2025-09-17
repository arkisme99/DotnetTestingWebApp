using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetTestingWebApp.Models
{
    public class Permission
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public ICollection<ApplicationRolePermission>? RolePermissions { get; set; }
    }
}