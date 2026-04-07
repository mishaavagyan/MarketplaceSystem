using PhoneService.BLL.Models.DTO;
using PhoneService.BLL.Models.ResponseModels;
using PhoneService.DLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneService.BLL.Abstractions
{
    public interface IPhoneService
    {   
        public Task<ResponseModel> CreateAsync(Guid userId,PhoneCreateDTO phoneCreate);
        public Task<ResponseModel> UpdateAsync(Guid productId,Guid userId,PhoneUpdateDTO phoneUpdate);
        public Task<ResponseModel> RemoveAsync(Guid productId,Guid userId); 
        public Task<ResponseModelWithBody<PhoneViewDTO>> GetAsync(Guid productId);
        public Task<ResponseModelWithBody<IEnumerable<PhoneViewDTO>>> GetAllAsync(FiltrModel filtrModel);
         
    }
}
    