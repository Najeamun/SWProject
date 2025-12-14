using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace SWProject.ApiService.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;

            // appsettings.json에서 SMTP 설정 로드
            _smtpServer = _configuration["SmtpSettings:Server"];
            _smtpPort = int.Parse(_configuration["SmtpSettings:Port"] ?? "587");
            _smtpUsername = _configuration["SmtpSettings:Username"];
            _smtpPassword = _configuration["SmtpSettings:Password"];
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var message = new MailMessage(_smtpUsername, toEmail, subject, body);
            message.IsBodyHtml = true;

            using (var smtpClient = new SmtpClient(_smtpServer, _smtpPort))
            {
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);

                try
                {
                    await smtpClient.SendMailAsync(message);
                    Console.WriteLine($"[EmailService] 이메일 전송 성공: {toEmail}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[EmailService] 이메일 전송 실패: {ex.Message}");
                    throw;
                }
            }
        }
    }
}
