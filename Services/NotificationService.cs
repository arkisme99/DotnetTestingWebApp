using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetTestingWebApp.Data;
using DotnetTestingWebApp.Hubs;
using DotnetTestingWebApp.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DotnetTestingWebApp.Services
{
    public class NotificationService(ApplicationDbContext _context, IHubContext<NotificationHub> _hub) : INotificationService
    {
        public async Task AddNotificationAsync(string userId, string message, string? fileUrl = null, string iconNotif = "fas fa-info")
        {
            var notif = new Notification
            {
                UserId = userId,
                Message = message,
                FileUrl = fileUrl
            };

            _context.Notifications.Add(notif);
            await _context.SaveChangesAsync();

            // Kirim real-time hanya ke user yang bersangkutan
            await _hub.Clients.Group(userId).SendAsync("ReceiveNotification", new
            {
                message = notif.Message,
                fileUrl = notif.FileUrl,
                icon = iconNotif,
                time = DateTime.Now.ToString("HH:mm")
            });
        }

        public async Task<List<Notification>> GetUnreadAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(int id, string userId)
        {
            var notif = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (notif != null)
            {
                notif.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

    }
}