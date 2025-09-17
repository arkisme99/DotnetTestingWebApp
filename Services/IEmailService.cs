using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetTestingWebApp.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(int senderId, string to, string subject, string body);
    }
}