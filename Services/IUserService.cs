using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetTestingWebApp.Models;
using DotnetTestingWebApp.Models.Dto;

namespace DotnetTestingWebApp.Services
{
    public interface IUserService
    {
        IQueryable<UserListDto> GetAll();
        Task<ApplicationUser> GetByidAsync(string id);
        Task<List<SelectTwoDto>> GetRoleByidUserAsync(string id);
        Task<ApplicationUser> StoreAsync(UserCreateDto dto);
        Task<ApplicationUser> UpdateAsync(string userId, UserCreateDto dto);
        Task DeleteAsync(string id);
    }
}