using AuthService.BLL.Models;
using AuthService.BLL.ResponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.BLL.Abstractions
{
    public interface IAuthService
    {
        public Task<ResponseModel> RegisterAsync(UserDTO userDTO);
        public Task<ResponseModelJwt> LoginAsync (UserDTO userDTO);
        public  Task<ResponseModel> VerifyEmailAsync(string email, string code);
    }
}
