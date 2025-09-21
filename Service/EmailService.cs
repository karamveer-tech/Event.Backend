using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;
using eventManager.Model;

namespace eventManager.Service
{
    public interface IEmailService
    {
        Task<string> SendEmailAsync(string toEmail, string subject, string body);
    }
    public class EmailService : IEmailService
    {
        private readonly EmailModel _settings;

        public EmailService(IOptions<EmailModel> settings)
        {
            _settings = settings.Value;
        }

        public async Task<string> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var senderEmail = new MailAddress(_settings.SenderEmail, _settings.DisplayName);

                // add footer with timestamp
                string footerTime = DateTime.UtcNow.ToLongTimeString();
                body = body + "<br/><br/>" + footerTime;

                using (var smtp = new SmtpClient
                {
                    Host = _settings.SmtpHost,
                    Port = _settings.Port,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_settings.SenderEmail, _settings.Password)
                })
                {
                    using (var mess = new MailMessage(senderEmail.Address, toEmail)
                    {
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    })
                    {
                        await smtp.SendMailAsync(mess);
                    }
                }

                return "Email sent successfully!";
            }
            catch (Exception ex)
            {
                return $"Error sending email: {ex.Message}";
            }
        }
    }
}
