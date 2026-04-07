using AuthService.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.BLL.Abstractions
{
    public interface IJwtService
    {
        public string GenerateToken(User user);
    }
}
