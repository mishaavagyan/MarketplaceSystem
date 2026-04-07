using OrderService.BLL.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.BLL.Abstractions
{
    public interface IRabbitMQPublisher
    {
        public Task PublishOrderCreated(OrderCreatedEvent orderCreated);
        public Task PublishOrderUpdated(OrderUpdatedEvent orderUpdated);
    }   
}       
