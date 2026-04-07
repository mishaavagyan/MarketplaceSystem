using PhoneService.DLL.Abstractions;
using PhoneService.DLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneService.DLL.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly PhoneDbContext _context;
        public UnitOfWork(PhoneDbContext context)
        {
            _context = context;
        }
        private IRepository<Phone> ?_phoneRepo;
        public IRepository<Phone> Phones
        {
            get
            {
                _phoneRepo ??= new Repository<Phone>(_context);
                return _phoneRepo;
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
