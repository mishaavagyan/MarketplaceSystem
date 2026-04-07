using Microsoft.EntityFrameworkCore;
using OrderService.DAL.Abstractions;
using OrderService.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.DAL.Implementations
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly OrderDbContext _context;
        private readonly DbSet<T> _dbSet;
        public Repository(OrderDbContext context)
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

        public IQueryable<T> GetAllQueryable()
        {
            return _dbSet.AsQueryable().AsNoTracking();
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
