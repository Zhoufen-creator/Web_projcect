using DoAnWeb.Data;
using DoAnWeb.Models;
using DoAnWeb.Services.Interface;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace DoAnWeb.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ApplicationDbContext _context;

        public EmailService(IOptions<EmailSettings> emailSettings, ApplicationDbContext context)
        {
            _emailSettings = emailSettings.Value;
            _context = context;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, string? userId = null)
        {
            var emailHistory = new EmailHistory
            {
                UserId = userId ?? string.Empty,
                EmailType = subject,
                Content = body,
                SentAt = DateTime.Now
            };

            try
            {
                using var mail = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mail.To.Add(toEmail);

                using var smtp = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
                {
                    Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                    EnableSsl = _emailSettings.EnableSsl
                };

                await smtp.SendMailAsync(mail);

                _context.EmailHistories.Add(emailHistory);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                emailHistory.Content += $"\n\n[EMAIL ERROR]: {ex.Message}";
                _context.EmailHistories.Add(emailHistory);
                await _context.SaveChangesAsync();
                return false;
            }
        }
    }
}
