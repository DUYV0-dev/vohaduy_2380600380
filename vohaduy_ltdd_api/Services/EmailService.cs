using System.Net;
using System.Net.Mail;

namespace vohaduy_ltdd_api.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string bodyHtml)
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings");
            bool enableSmtp = smtpSettings.GetValue<bool>("EnableSmtp");

            if (!enableSmtp)
            {
                _logger.LogInformation("================== [MOCK EMAIL SERVICE] ==================");
                _logger.LogInformation("To: {ToEmail}", toEmail);
                _logger.LogInformation("Subject: {Subject}", subject);
                _logger.LogInformation("Body: {Body}", bodyHtml);
                _logger.LogInformation("==========================================================");
                return true;
            }

            try
            {
                var server = smtpSettings["Server"] ?? "smtp.gmail.com";
                var port = smtpSettings.GetValue<int>("Port", 587);
                var senderEmail = smtpSettings["SenderEmail"] ?? "vohaduy.attendance@gmail.com";
                var senderName = smtpSettings["SenderName"] ?? "Võ Hà Duy - Chấm Công";
                var password = smtpSettings["Password"] ?? "";

                using var client = new SmtpClient(server, port)
                {
                    Credentials = new NetworkCredential(senderEmail, password),
                    EnableSsl = true
                };

                using var message = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = subject,
                    Body = bodyHtml,
                    IsBodyHtml = true
                };
                message.To.Add(toEmail);

                await client.SendMailAsync(message);
                _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {ToEmail}", toEmail);
                return false;
            }
        }
    }
}
