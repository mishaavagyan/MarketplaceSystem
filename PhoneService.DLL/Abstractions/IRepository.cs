using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PhoneService.DLL.Abstractions
{
    public interface IRepository<T> where T : class
    {
        public Task AddAsync(T entity);
        public void Update(T entity);
        public void Delete(T entity);
        public Task<IEnumerable<T>> GetAllAsync();      
        public IQueryable<T> GetAllQueryable();    
        public Task<T?> GetAsync(Expression<Func<T, bool>> predicate);
    }
}
