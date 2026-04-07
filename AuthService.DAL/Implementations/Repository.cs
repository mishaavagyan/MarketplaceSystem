using AuthService.DAL.Abstractions;
using AuthService.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AuthService.DAL.Implementations
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly AuthDbContext _authDbContext;
        private readonly DbSet<T> _dbSet;
        public Repository(AuthDbContext authDbContext)
        {
            _authDbContext = authDbContext;
            _dbSet = _authDbContext.Set<T>();
        }
        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task<T?> GetAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }
    }
}
