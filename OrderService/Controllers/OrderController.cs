using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.BLL.Abstractions;
using OrderService.BLL.Models.DTO;
using OrderService.BLL.Models.ResponseModel;
using OrderService.Extensions;
using System.Diagnostics;
using System.Net;
using System.Security.Claims;

namespace OrderService.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]/[action]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;  
        public OrderController(IOrderService orderService , ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromQuery] Guid phoneId)
        {
            var userId = HttpContext.GetUserId();
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            _logger.LogInformation("Creating order for {UserId} from IP {IP}", userId, ip);

            var result = await _orderService.CreateOrderAsync(phoneId, userId);

            if (result.StatusCode == HttpStatusCode.BadRequest)
            {
                _logger.LogWarning("BadRequest for {UserId}: {Message}", userId, result.Message);
                return BadRequest(result.Message);
            }

            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("NotFound for {UserId}: {Message}", userId, result.Message);
                return NotFound(result.Message);
            }

            if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                _logger.LogError("Internal error for {UserId}", userId);
                return StatusCode(500, result.Message);
            }

            if (result.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                _logger.LogError("Service unavailable for {UserId}", userId);
                return StatusCode(503, result.Message);
            }

            _logger.LogInformation("Order created for {UserId}", userId);
            return Ok(result.Message);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = HttpContext.GetUserId();

            _logger.LogInformation("Getting orders for {UserId}", userId);

            var result = await _orderService.GetMyOrdersAsync(userId);

            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Orders not found for {UserId}", userId);
                return NotFound(result.Message);
            }

            if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                _logger.LogError("Error while getting orders for {UserId}", userId);
                return StatusCode(500, result.Message);
            }

            _logger.LogInformation("Orders retrieved for {UserId}", userId);
            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetMyOrdersToSell()
        {
            var userId = HttpContext.GetUserId();
                
            _logger.LogInformation("Getting orders for {UserId}", userId);

            var result = await _orderService.GetMyOrdersToSellAsync(userId);

            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Orders not found for {UserId}", userId);
                return NotFound(result.Message);
            }

            if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                _logger.LogError("Error while getting orders for {UserId}", userId);
                return StatusCode(500, result.Message);
            }

            _logger.LogInformation("Orders retrieved for {UserId}", userId);
            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetOrderByIdAsync([FromQuery] Guid orderId)
        {
            var userId = HttpContext.GetUserId();

            _logger.LogInformation("Getting order {OrderId} for {UserId}", orderId, userId);

            var result = await _orderService.GetOrderByIdAsync(orderId, userId);

            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Order not found {OrderId} for {UserId}", orderId, userId);
                return NotFound(result.Message);
            }

            if (result.StatusCode == HttpStatusCode.Forbidden)
            {
                _logger.LogWarning("Forbidden access to order {OrderId} for {UserId}", orderId, userId);
                return Forbid();
            }

            if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                _logger.LogError("Error while getting order {OrderId}", orderId);
                return StatusCode(500, result.Message);
            }

            _logger.LogInformation("Order retrieved {OrderId}", orderId);
            return Ok(result);
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteOrderAsync([FromQuery] Guid orderId)
        {
            var userId = HttpContext.GetUserId();

            _logger.LogInformation("Deleting order {OrderId} for {UserId}", orderId, userId);

            var result = await _orderService.DeleteOrderAsync(orderId, userId);

            if (result.StatusCode == HttpStatusCode.BadRequest)
            {
                _logger.LogWarning("BadRequest deleting order {OrderId}", orderId);
                return BadRequest(result.Message);
            }

            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Order not found {OrderId}", orderId);
                return NotFound(result.Message);
            }

            _logger.LogInformation("Order deleted {OrderId}", orderId);
            return Ok(result.Message);
        }
        [HttpPut]
        public async Task<IActionResult> UpdateOrderStatus([FromQuery] Guid orderId, [FromBody] OrderStatus status)
        {
            var userId = HttpContext.GetUserId();

            _logger.LogInformation("Updating order {OrderId} for {UserId} to {Status}", orderId, userId, status);

            var result = await _orderService.UpdateOrderStatus(orderId, userId, status);

            if (result.StatusCode == HttpStatusCode.BadRequest)
            {
                _logger.LogWarning("BadRequest updating order {OrderId}", orderId);
                return BadRequest(result.Message);
            }

            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Order not found {OrderId}", orderId);
                return NotFound(result.Message);
            }

            if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                _logger.LogError("Error updating order {OrderId}", orderId);
                return StatusCode(500, result.Message);
            }

            if (result.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                _logger.LogError("Service unavailable updating order {OrderId}", orderId);
                return StatusCode(503, result.Message);
            }

            _logger.LogInformation("Order updated {OrderId}", orderId);
            return Ok(result.Message);
        }

    }
}
