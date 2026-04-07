using Microsoft.EntityFrameworkCore;
using PhoneService.DLL.Abstractions;
using PhoneService.DLL.Models;
using System.Linq.Expressions;

namespace PhoneService.DLL.Implementations
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly PhoneDbContext _context;
        private readonly DbSet<T> _dbSet;
        public Repository(PhoneDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }
        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }
            
        public IQueryable<T> GetAllQueryable()
        {
            return _dbSet.AsQueryable();    
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
