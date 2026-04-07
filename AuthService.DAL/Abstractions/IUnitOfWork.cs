using AuthService.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.DAL.Abstractions
{
    public interface IUnitOfWork
    {
        public IRepository<User> Users { get; }
        public Task SaveChangesAsync();
    }
}
