using MimeKit;
using QuickDesk.worker.Infrastructure.Interfaces;
using System.Net;
using System.Net.Mail;

namespace QuickDesk.worker.Infrastructure
{
    public class EmailSenderService : IEmailSenderService
    {
        private readonly ILogger<EmailSenderService> _logger;
        private readonly IConfiguration _configuration;

        public EmailSenderService(ILogger<EmailSenderService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string message, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Sending email to: {To}, Subject: {Subject}, Message: {Message}", to, subject, message);
                var emailSettings = _configuration.GetSection("EmailSettings");
                var email = new MailMessage();

                email.From = new MailAddress(emailSettings["SenderEmail"], "QuickDesk");               
                email.Subject = subject;
                email.IsBodyHtml = true;
                email.Body = message;

                email.To.Add(to);

                using var client = new SmtpClient(emailSettings["SmtpServer"], int.Parse(emailSettings["Port"]))
                {
                    Credentials = new NetworkCredential(emailSettings["SenderEmail"], emailSettings["Password"]),
                    EnableSsl = true
                };

                await client.SendMailAsync(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in EmailSenderService:SendEmailAsync To:{to}, Subject:{subject}", to, subject);
                throw;
            }
        }
    }
}
