using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetTestingWebApp.Models
{
    public class ApplicationRolePermission
    {
        public string? RoleId { get; set; }
        public ApplicationRole? Role { get; set; }

        public int PermissionId { get; set; }
        public Permission? Permission { get; set; }
    }
}