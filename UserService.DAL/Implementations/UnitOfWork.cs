using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using UserService.DAL.Abstractions;
using UserService.DAL.Models;

namespace UserService.DAL.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {   
        private readonly UserDbContext _userDbContext;
        public UnitOfWork(UserDbContext userDbContext)
        {
            _userDbContext = userDbContext;
        }
        private IRepository<User>? _userRepo;
        public IRepository<User> Users
        {
            get
            {
                _userRepo ??= new Repository<User>(_userDbContext);
                return _userRepo;
            }
        }

        public async Task SaveChangesAsync()
        {
            await _userDbContext.SaveChangesAsync();
        }
    }
}
