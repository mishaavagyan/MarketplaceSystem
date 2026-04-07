using Microsoft.Identity.Client;
using OrderService.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.DAL.Abstractions
{
    public interface IUnitOfWork
    {
        public Task SaveChangesAsync();
        public IRepository<Order> Order { get; }
    }
}
