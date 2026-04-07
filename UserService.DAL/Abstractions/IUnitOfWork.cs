using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.DAL.Models;

namespace UserService.DAL.Abstractions
{
    public interface IUnitOfWork
    {
        public IRepository<User> Users { get; }
        public Task SaveChangesAsync();
    }
}
