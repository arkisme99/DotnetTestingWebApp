using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetTestingWebApp.Models.Dto
{
    public class UserListDto
    {
        public string Id { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string UserName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string DeletedAt { get; set; } = default!;
        public List<string> Roles { get; set; } = [];
    }

}