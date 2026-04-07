using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using UserService.DAL.Models;

namespace UserService.DAL.Abstractions
{
    public interface IRepository<T> where T : class
    {
        public Task<T?> GetAsync(Expression<Func<T,bool>> predicate); 
        public void Update(T item); 
        public Task CreateAsync(T item);
            
    }
}   
    