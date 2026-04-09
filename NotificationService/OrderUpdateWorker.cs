using NotificationService.Interfaces;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using NotificationService.Models;

namespace NotificationService
{
    public class OrderUpdateWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IEmailSender _emailSender;
        ILogger<OrderUpdateWorker> _logger;

        public OrderUpdateWorker(IServiceProvider serviceProvider, IEmailSender emailSender, ILogger<OrderUpdateWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _emailSender = emailSender;
            _logger = logger;
        }
            
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Worker OrderUpdate Starting.");
                var factory = new ConnectionFactory()
                {
                    HostName = "localhost",
                    UserName = "guest",
                    Password = "guest",
                };

                var connection = await factory.CreateConnectionAsync();
                var channel = await connection.CreateChannelAsync();

                await channel.QueueDeclareAsync(queue: "order.updated", durable: false, exclusive: false, autoDelete: false, arguments: null);

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (model, ea) =>
                {
                    using var scope = _serviceProvider.CreateScope();

                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var orderUpdated = JsonSerializer.Deserialize<OrderUpdatedEvent>(message);

                    if (orderUpdated is not null)
                    {
                        await _emailSender.SendEmailAsync(orderUpdated.BuyerEmail, "Order Updated", $"Order {orderUpdated.Id} now has status {orderUpdated.Status}");
                        await _emailSender.SendEmailAsync(orderUpdated.SellerEmail, "Order Updated", $"Order {orderUpdated.Id} status is now {orderUpdated.Status}");
                    }
                };

                await channel.BasicConsumeAsync(queue: "order.updated", autoAck: true, consumer: consumer);
                await Task.Delay(Timeout.Infinite, stoppingToken);
                _logger.LogInformation("Worker OrderUpdate Working");

            }
            catch (Exception ex)
            {
                _logger.LogError($"Worker OrderUpadate Crashed {ex.Message}");
            }
           
        }
    }
}
