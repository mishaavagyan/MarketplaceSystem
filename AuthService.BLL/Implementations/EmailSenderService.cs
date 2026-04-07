using AuthService.BLL.Abstractions;
using AuthService.BLL.Models;
using AuthService.BLL.ResponseModels;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuthService.BLL.Implementations
{

    public class EmailSenderService : IEmailSenderService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailSettings> _logger;
        private readonly IRedisService _redisService;   
        public EmailSenderService(EmailSettings emailSettings,ILogger<EmailSettings> logger, IRedisService redisService)
        {
            _emailSettings = emailSettings;
            _logger = logger;
            _redisService = redisService;   
        }
        public async Task SendEmail(string email)
        {
            try
            {
                var code = GenerateCode();
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(_emailSettings.Email, _emailSettings.Password),
                    EnableSsl = true,
                };

                var message = new MailMessage
                {
                    From = new MailAddress(_emailSettings.Email, "Marketplace"),
                    Subject = "Your Verification Code",
                    Body = $"Your verification code is: {code}",
                    IsBodyHtml = true,
                };

                message.To.Add(email);
                _logger.LogInformation("Sending Verfication Email to {Email}", email);
                await smtpClient.SendMailAsync(message);
                await _redisService.SetVerificationCodeAsync(email, code.ToString(), TimeSpan.FromMinutes(30));
                _logger.LogInformation("Stored Verification Code in Redis For {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed To Send Verification code to {Email} ", email);
            }
        }
        public static int GenerateCode()
        {
            Random random = new Random();
            return random.Next(100000, 999999);
        }
    }
}
