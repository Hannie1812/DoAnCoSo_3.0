//using MailKit.Net.Smtp;
//using MimeKit;
//using Microsoft.Extensions.Configuration;
//using System.Net.Security;
//using System.Security.Cryptography.X509Certificates;

//namespace WebTimNguoiThatLac.Services
//{


//    public class EmailService
//    {
//        private readonly IConfiguration _configuration;

//        public EmailService(IConfiguration configuration)
//        {
//            _configuration = configuration;
//        }

//        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false)
//        {
//            var emailSettings = _configuration.GetSection("EmailSettings");

//            var message = new MimeMessage();
//            message.From.Add(new MailboxAddress(emailSettings["FromName"], emailSettings["FromAddress"]));
//            message.To.Add(new MailboxAddress("", toEmail));
//            message.Subject = subject;

//            var bodyBuilder = new BodyBuilder();
//            if (isHtml)
//            {
//                bodyBuilder.HtmlBody = body;
//            }
//            else
//            {
//                bodyBuilder.TextBody = body;
//            }

//            message.Body = bodyBuilder.ToMessageBody();

//            using (var client = new SmtpClient())
//            {
//                // Bỏ qua kiểm tra chứng chỉ SSL
//                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

//                await client.ConnectAsync(emailSettings["SmtpServer"],
//                                          int.Parse(emailSettings["SmtpPort"]),
//                                          MailKit.Security.SecureSocketOptions.StartTls);
//                await client.AuthenticateAsync(emailSettings["SmtpUsername"],
//                                               emailSettings["SmtpPassword"]);
//                await client.SendAsync(message);
//                await client.DisconnectAsync(true);
//            }

//        }
//    }
//}


using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;


namespace WebTimNguoiThatLac.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(toEmail))
                    throw new ArgumentException("Email người nhận không được để trống");

                var emailSettings = _configuration.GetSection("EmailSettings");

                // Validate configuration
                if (emailSettings == null)
                    throw new InvalidOperationException("Cấu hình email không tồn tại");

                var smtpServer = emailSettings["SmtpServer"];
                var smtpPort = emailSettings["SmtpPort"];
                var username = emailSettings["SmtpUsername"];
                var password = emailSettings["SmtpPassword"];

                if (string.IsNullOrWhiteSpace(smtpServer) || string.IsNullOrWhiteSpace(smtpPort) ||
                    string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                    throw new InvalidOperationException("Thiếu thông tin cấu hình SMTP");

                // Prepare email message
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(
                    emailSettings["FromName"] ?? "Hệ thống Tìm Người Thất Lạc",
                    emailSettings["FromAddress"] ?? username
                ));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder();
                if (isHtml)
                    bodyBuilder.HtmlBody = body;
                else
                    bodyBuilder.TextBody = body;

                message.Body = bodyBuilder.ToMessageBody();

                // Send email
                using (var client = new SmtpClient())
                {
                    // Only bypass SSL certificate validation in development
                    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                    {
                        client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    }

                    await client.ConnectAsync(
                        smtpServer,
                        int.Parse(smtpPort),
                        MailKit.Security.SecureSocketOptions.StartTlsWhenAvailable // More flexible than StartTls
                    );

                    await client.AuthenticateAsync(username, password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                _logger.LogInformation($"Email gửi tới {toEmail} thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi gửi email tới {toEmail}");
                throw; // Re-throw để controller có thể bắt và xử lý
            }
        }
    }
}