using OrderService.DAL.Abstractions;
using OrderService.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.DAL.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly OrderDbContext _context;
        public UnitOfWork(OrderDbContext context)
        {
            _context = context;   
        }
        private IRepository<Order> ?_orderRepo;
        public IRepository<Order> Order
        {
            get 
            {
                _orderRepo ??= new Repository<Order>(_context);
                return _orderRepo;
            }
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
