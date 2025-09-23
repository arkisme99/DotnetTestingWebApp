using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DotnetTestingWebApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotnetTestingWebApp.Controllers
{
    public class NotificationController(INotificationService _service) : Controller
    {
        [HttpGet, ActionName("unread")]
        public async Task<IActionResult> GetUnread()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var notifs = await _service.GetUnreadAsync(userId);
            return Ok(notifs.Select(n => new
            {
                n.Id,
                n.Message,
                n.FileUrl,
                Time = n.CreatedAt?.ToString("HH:mm"),
                n.IsRead
            }));
        }

        [HttpPost, ActionName("read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            await _service.MarkAsReadAsync(id, userId);
            return Ok();
        }
    }
}