using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetTestingWebApp.Models.Dto
{
    public class UserCreateDto
    {
        public string UserName { get; set; } = default!;
        public string Fullname { get; set; } = default!;
        public string? Password { get; set; }
        public string? Email { get; set; }
        public List<string>? Roles { get; set; }
    }

}