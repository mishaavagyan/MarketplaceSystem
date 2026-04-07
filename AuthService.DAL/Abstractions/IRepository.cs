using System.Linq.Expressions;

namespace AuthService.DAL.Abstractions
{
    public interface IRepository<TEntity> where TEntity : class
    {
        public Task AddAsync(TEntity entity);
        public void Update(TEntity entity);
        public Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate);
    }
}   
