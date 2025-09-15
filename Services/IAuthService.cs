using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetTestingWebApp.Services
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(string email, string password);
        Task LogoutAsync();
        Task<bool> RegisterAsync(string email, string password);
        Task<bool> AssignRoleAsync(string email, string roleName);
    }
}