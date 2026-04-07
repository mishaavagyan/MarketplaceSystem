using AuthService.DAL.Abstractions;
using AuthService.DAL.Models;

namespace AuthService.DAL.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AuthDbContext _authDbContext;
        public UnitOfWork(AuthDbContext authDbContext)
        {
            _authDbContext = authDbContext;
        }
        private IRepository<User>? _userRepo;    
        public IRepository<User> Users 
        {
            get
            {
                _userRepo ??= new Repository<User>(_authDbContext);
                return _userRepo;
            }
        }

        public async Task SaveChangesAsync()
        {
            await _authDbContext.SaveChangesAsync();  
        }
    }
}
