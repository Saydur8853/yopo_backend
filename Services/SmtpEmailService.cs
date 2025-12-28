using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace YopoBackend.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
        {
            _logger = logger;
            _settings = EmailSettings.FromConfiguration(configuration);
        }

        public async Task SendAsync(string toEmail, string subject, string htmlBody, string? textBody = null)
        {
            if (string.IsNullOrWhiteSpace(_settings.SmtpHost) || string.IsNullOrWhiteSpace(_settings.FromAddress))
            {
                throw new InvalidOperationException("Email settings are not configured.");
            }

            using var message = new MailMessage
            {
                From = new MailAddress(_settings.FromAddress, _settings.FromName),
                Subject = subject,
                Body = string.IsNullOrWhiteSpace(htmlBody) ? textBody ?? string.Empty : htmlBody,
                IsBodyHtml = !string.IsNullOrWhiteSpace(htmlBody)
            };
            message.To.Add(new MailAddress(toEmail));

            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                EnableSsl = _settings.EnableSsl
            };

            if (!string.IsNullOrWhiteSpace(_settings.SmtpUser))
            {
                client.Credentials = new NetworkCredential(_settings.SmtpUser, _settings.SmtpPass);
            }

            try
            {
                await client.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                throw;
            }
        }

        private sealed class EmailSettings
        {
            public string SmtpHost { get; init; } = string.Empty;
            public int SmtpPort { get; init; } = 587;
            public string SmtpUser { get; init; } = string.Empty;
            public string SmtpPass { get; init; } = string.Empty;
            public string FromAddress { get; init; } = string.Empty;
            public string FromName { get; init; } = "Yopo";
            public bool EnableSsl { get; init; } = true;

            public static EmailSettings FromConfiguration(IConfiguration configuration)
            {
                var section = configuration.GetSection("Email");
                return new EmailSettings
                {
                    SmtpHost = section["SmtpHost"] ?? string.Empty,
                    SmtpPort = int.TryParse(section["SmtpPort"], out var port) ? port : 587,
                    SmtpUser = section["SmtpUser"] ?? string.Empty,
                    SmtpPass = section["SmtpPass"] ?? string.Empty,
                    FromAddress = section["FromAddress"] ?? string.Empty,
                    FromName = section["FromName"] ?? "Yopo",
                    EnableSsl = !bool.TryParse(section["EnableSsl"], out var enableSsl) || enableSsl
                };
            }
        }
    }
}
