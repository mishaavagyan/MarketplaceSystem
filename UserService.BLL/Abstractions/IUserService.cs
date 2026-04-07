using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.BLL.Models;
using UserService.BLL.ResponseModels;
using UserService.DAL.Models;

namespace UserService.BLL.Abstractions
{
    public interface IUserService   
    {
        public Task<ResponseModel> CreateAsync(UserCreatedEvent user);
        public Task<ResponseModel> UpdateAsync(Guid id,UserUpdateDTO user);
        public Task<ResponseModelWithBody<UserViewDTO>> GetById(Guid guid);
    }
}
