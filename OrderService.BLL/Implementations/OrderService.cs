using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderService.BLL.Abstractions;
using OrderService.BLL.HttpCaller;
using OrderService.BLL.Models.DTO;
using OrderService.BLL.Models.ResponseModel;
using OrderService.DAL.Abstractions;
using OrderService.DAL.Models;
using System.Net;

namespace OrderService.BLL.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _context;
        private readonly HttpsCaller _client;
        private readonly IRabbitMQPublisher _publisher;
        private readonly ILogger<OrderService> _logger;
        public OrderService(IUnitOfWork context, HttpsCaller client, IRabbitMQPublisher publisher, ILogger<OrderService> logger)
        {
            _context = context;
            _client = client;
            _publisher = publisher;
            _logger = logger;
        }

        public async Task<Response> CreateOrderAsync(Guid phoneId, Guid buyerId)
        {
            _logger.LogInformation("Order creation started for Buyer {BuyerId}", buyerId);

            if (phoneId == Guid.Empty)
                return new Response { Message = "Invalid Phone Id", StatusCode = HttpStatusCode.BadRequest };

            var existingOrder = await _context.Order.GetAsync(x => x.PhoneId == phoneId);
            if (existingOrder is not null)
                return new Response { Message = "Order with this phone already exists", StatusCode = HttpStatusCode.BadRequest };

            var phone = await _client.GetPhoneByIdAsync(phoneId);
            if (phone is null)
                return new Response { Message = "Phone not found", StatusCode = HttpStatusCode.NotFound };

            if (phone.OwnerId == buyerId)
                return new Response { Message = "You can't buy your own phone", StatusCode = HttpStatusCode.BadRequest };

            UserDTO? buyer;
            UserDTO? seller;

            try
            {
                var buyerTask = _client.GetUserByIdAsync(buyerId);
                var sellerTask = _client.GetUserByIdAsync(phone.OwnerId);

                await Task.WhenAll(buyerTask, sellerTask);

                buyer = buyerTask.Result;
                seller = sellerTask.Result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "User service error");
                return new Response { Message = "User service unavailable", StatusCode = HttpStatusCode.ServiceUnavailable };
            }

            if (buyer is null || seller is null)
                return new Response { Message = "Buyer or Seller not found", StatusCode = HttpStatusCode.NotFound };

            var order = new Order
            {
                Id = Guid.NewGuid(),
                BuyerId = buyerId,
                SellerId = phone.OwnerId,
                PhoneId = phoneId,
                CreatedAt = DateTime.UtcNow,
                Status = (int)OrderStatus.Pending
            };

            await _context.Order.AddAsync(order);
            await _context.SaveChangesAsync();

            try
            {
                await _publisher.PublishOrderCreated(new OrderCreatedEvent
                {
                    Id = order.Id,
                    BuyerId = order.BuyerId,
                    SellerId = order.SellerId,
                    PhoneId = order.PhoneId,
                    CreatedAt = order.CreatedAt,
                    BuyerEmail = buyer.Email,
                    SellerEmail = seller.Email
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RabbitMQ error");
                return new Response { Message = "Message broker error", StatusCode = HttpStatusCode.InternalServerError };
            }

            _logger.LogInformation("Order created successfully for {BuyerId}", buyerId);

            return new Response
            {
                Message = "Order created successfully",
                StatusCode = HttpStatusCode.OK
            };
        }


        public async Task<Response> DeleteOrderAsync(Guid orderId, Guid buyerId)
        {
            _logger.LogInformation("Deleting order {OrderId}", orderId);

            if (orderId == Guid.Empty)
                return new Response { Message = "Invalid Order Id", StatusCode = HttpStatusCode.BadRequest };

            var order = await _context.Order.GetAsync(x => x.Id == orderId);

            if (order is null)
                return new Response { Message = "Order not found", StatusCode = HttpStatusCode.NotFound };

            if (order.BuyerId != buyerId)
                return new Response { Message = "You can't delete other order", StatusCode = HttpStatusCode.BadRequest };

            if (order.Status == (int)OrderStatus.Confirmed)
                return new Response { Message = "Confirmed order can't be deleted", StatusCode = HttpStatusCode.BadRequest };

            _context.Order.Delete(order);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Order deleted {OrderId}", orderId);

            return new Response
            {
                Message = "Order deleted successfully",
                StatusCode = HttpStatusCode.OK
            };
        }

        public async Task<ResponseWithBody<IEnumerable<OrderViewDTO>>> GetMyOrdersAsync(Guid userId)
        {
            try
            {
                _logger.LogInformation("receiving of orders begins");
                var orders = _context.Order.GetAllQueryable().Where(x => x.BuyerId == userId);
                var result = await orders.Select(x => new OrderViewDTO
                {

                    BuyerId = x.BuyerId,
                    Id = x.Id,
                    PhoneId = x.PhoneId,
                    SellerId = x.SellerId,
                    CreatedAt = x.CreatedAt,
                    Status = x.Status
                }).ToListAsync();

                _logger.LogInformation("Orders Successfully Received");
                if (result.Any())
                {
                    return new ResponseWithBody<IEnumerable<OrderViewDTO>> { Body = result, Message = "Success", StatusCode = HttpStatusCode.OK };
                }
                return new ResponseWithBody<IEnumerable<OrderViewDTO>> { Message = "You didn't have orders", StatusCode = HttpStatusCode.NotFound };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new ResponseWithBody<IEnumerable<OrderViewDTO>> { Message = "Internal Server Error", StatusCode = HttpStatusCode.InternalServerError };
            }

        }

        public async Task<ResponseWithBody<OrderViewDTO>> GetOrderByIdAsync(Guid orderId, Guid userId)
        {
            _logger.LogInformation("Getting order {OrderId}", orderId);

            var order = await _context.Order.GetAsync(x => x.Id == orderId);

            if (order is null)
                return new ResponseWithBody<OrderViewDTO>
                {
                    Message = "Not Found",
                    StatusCode = HttpStatusCode.NotFound
                };

            if (order.SellerId != userId && order.BuyerId != userId)
                return new ResponseWithBody<OrderViewDTO>
                {
                    Message = "This is not your order",
                    StatusCode = HttpStatusCode.Forbidden
                };

            var dto = new OrderViewDTO
            {
                Id = order.Id,
                BuyerId = order.BuyerId,
                SellerId = order.SellerId,
                PhoneId = order.PhoneId,
                CreatedAt = order.CreatedAt,
                Status = order.Status
            };

            return new ResponseWithBody<OrderViewDTO>
            {
                Body = dto,
                Message = "Success",
                StatusCode = HttpStatusCode.OK
            };
        }

        public async Task<ResponseWithBody<IEnumerable<OrderViewDTO>>> GetMyOrdersToSellAsync(Guid userId)
        {
            try
            {
                _logger.LogInformation("receiving of orders begins");
                var orders = _context.Order.GetAllQueryable().Where(x => x.SellerId == userId);
                var result = await orders.Select(x => new OrderViewDTO
                {

                    BuyerId = x.BuyerId,
                    Id = x.Id,
                    PhoneId = x.PhoneId,
                    SellerId = x.SellerId,
                    CreatedAt = x.CreatedAt,
                    Status = x.Status
                }).ToListAsync();

                _logger.LogInformation("Orders Successfully Received");
                if (result.Any())
                {
                    return new ResponseWithBody<IEnumerable<OrderViewDTO>> { Body = result, Message = "Success", StatusCode = HttpStatusCode.OK };
                }
                return new ResponseWithBody<IEnumerable<OrderViewDTO>> { Message = "You didn't have orders", StatusCode = HttpStatusCode.NotFound };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new ResponseWithBody<IEnumerable<OrderViewDTO>> { Message = "Internal Server Error", StatusCode = HttpStatusCode.InternalServerError };
            }

        }

        public async Task<Response> UpdateOrderStatus(Guid orderId, Guid userId, OrderStatus status)
        {
            _logger.LogInformation("Updating order {OrderId}", orderId);

            var order = await _context.Order.GetAsync(x => x.Id == orderId);

            if (order is null)
                return new Response { Message = "Order not found", StatusCode = HttpStatusCode.NotFound };

            if (order.SellerId != userId && order.BuyerId != userId)
                return new Response { Message = "You can't change other order", StatusCode = HttpStatusCode.BadRequest };

            if (order.Status == (int)OrderStatus.Pending && status == OrderStatus.Cancelled)
                return new Response { Message = "Pending order can't be cancelled", StatusCode = HttpStatusCode.BadRequest };

            if (order.Status == (int)OrderStatus.Cancelled &&
                (status == OrderStatus.Confirmed || status == OrderStatus.Pending))
                return new Response { Message = "Cancelled order can't be changed", StatusCode = HttpStatusCode.BadRequest };

            order.Status = (int)status;

            _context.Order.Update(order);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Order updated {OrderId}", orderId);

            return new Response
            {
                Message = "Changed successfully",
                StatusCode = HttpStatusCode.OK
            };
        }

    }
}
