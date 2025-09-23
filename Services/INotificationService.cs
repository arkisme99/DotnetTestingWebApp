using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetTestingWebApp.Models;

namespace DotnetTestingWebApp.Services
{
    public interface INotificationService
    {
        Task AddNotificationAsync(string userId, string message, string? fileUrl = null, string iconNotif = "fas fa-info");
        Task<List<Notification>> GetUnreadAsync(string userId);
        Task MarkAsReadAsync(int id, string userId);
    }
}