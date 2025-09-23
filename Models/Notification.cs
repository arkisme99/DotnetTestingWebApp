using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetTestingWebApp.Models
{
    public class Notification : AuditableEntity
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty; // aspnetusers id
        public string Message { get; set; } = string.Empty;
        public string? FileUrl { get; set; }
        public bool IsRead { get; set; } = false;
    }
}