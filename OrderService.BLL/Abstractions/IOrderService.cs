using OrderService.BLL.Models.DTO;
using OrderService.BLL.Models.ResponseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.BLL.Abstractions
{
    public interface IOrderService
    {
        public Task<Response> CreateOrderAsync(Guid phoneId, Guid buyerid);
        public Task<ResponseWithBody<IEnumerable<OrderViewDTO>>> GetMyOrdersAsync(Guid userId);
        public Task<ResponseWithBody<IEnumerable<OrderViewDTO>>> GetMyOrdersToSellAsync(Guid userId);
        public Task<ResponseWithBody<OrderViewDTO>> GetOrderByIdAsync(Guid orderId, Guid userId);   
        public Task<Response> DeleteOrderAsync(Guid orderId, Guid buyerid); 
        public Task<Response> UpdateOrderStatus(Guid orderId, Guid userId, OrderStatus status);
    }
}
