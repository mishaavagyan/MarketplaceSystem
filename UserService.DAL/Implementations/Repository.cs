using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using UserService.DAL.Abstractions;
using UserService.DAL.Models;

namespace UserService.DAL.Implementations
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly UserDbContext _userDbContext;
        private readonly DbSet<T> _dbSet;
        public Repository(UserDbContext userDbContext)
        {
            _userDbContext = userDbContext;
            _dbSet = _userDbContext.Set<T>();
        }

        public async Task CreateAsync(T item)
        {
            await _dbSet.AddAsync(item);
        }

        public async Task<T?> GetAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);

        }

        public void Update(T item)
        {   
            _dbSet.Update(item);
        }
    }
}
