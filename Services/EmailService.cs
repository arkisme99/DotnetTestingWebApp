using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;

namespace DotnetTestingWebApp.Services
{
    public class EmailService : IEmailService
    {
        public async Task SendEmailAsync(int senderId, string to, string subject, string body)
        {
            /* var sender = await _db.EmailSenders.FindAsync(senderId);
            if (sender == null) throw new Exception("Sender tidak ditemukan"); */

            var sender = "test@email.com";
            var username = "4df6bdb48d7e0e";
            var senderPassword = "c9cd90c86fee8f";
            var smtpServer = "sandbox.smtp.mailtrap.io";
            var smtpPort = 587;

            var email = new MimeMessage();
            // email.From.Add(MailboxAddress.Parse(sender.Email));
            email.From.Add(MailboxAddress.Parse(sender));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;

            email.Body = new TextPart("plain") { Text = body };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(username, senderPassword);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
            /* await smtp.ConnectAsync(sender.SmtpServer, sender.Port, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(sender.Email, sender.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true); */
        }

    }
}