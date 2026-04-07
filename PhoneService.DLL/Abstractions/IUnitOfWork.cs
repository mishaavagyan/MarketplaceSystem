using PhoneService.DLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneService.DLL.Abstractions
{
    public interface IUnitOfWork
    {
        public IRepository<Phone> Phones { get;}
        public Task SaveChangesAsync();
    }
}
