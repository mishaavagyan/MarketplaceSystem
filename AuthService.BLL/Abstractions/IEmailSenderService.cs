using AuthService.BLL.ResponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.BLL.Abstractions
{
    public interface IEmailSenderService
    {
        public Task SendEmail(string email);
    }
}
