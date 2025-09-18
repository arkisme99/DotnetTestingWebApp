using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetTestingWebApp.Models.ViewModels
{
    public class EditRoleViewModel<T>
    {
        public T? Data { get; set; }
        public Dictionary<string, List<Permission>>? Permissions { get; set; }
        public List<Permission>? CurrentRolePermissions { get; set; }
    }
}