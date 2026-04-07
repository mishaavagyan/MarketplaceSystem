using NotificationService.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NotificationService.Models;

namespace NotificationService.Implementations
{
    public class EmailSender : IEmailSender
    {
        public async Task<ResponseModel> SendEmailAsync(string email,string subject, string model)
        {   
            try
            {   
                
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("mishoavagyan313@gmail.com", "ajyojhvjnqjyitsm"),
                    EnableSsl = true,
                };

                var message = new MailMessage
                {
                    From = new MailAddress("mishoavagyan313@gmail.com", "Marketplace"),
                    Subject = subject,
                    Body = model,
                    IsBodyHtml = true,
                };

                message.To.Add(email);  
                await smtpClient.SendMailAsync(message);
                return new ResponseModel { Message = "Success", StatusCode = HttpStatusCode.OK };
            }
            catch (Exception ex)
            {
                return new ResponseModel { Message = "Can't Send Email Invalid Email", StatusCode = HttpStatusCode.NotFound };
            }

        }
    }
}
