using NotificationService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService.Interfaces
{
    public interface IEmailSender
    {
        public Task<ResponseModel> SendEmailAsync(string email,string subject,string model);
    }
}
    