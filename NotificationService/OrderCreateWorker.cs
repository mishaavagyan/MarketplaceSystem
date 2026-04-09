using NotificationService.Interfaces;
using NotificationService.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace NotificationService
{
    public class OrderCreateWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IEmailSender _emailSender;
        ILogger<OrderCreateWorker> _logger;

        public OrderCreateWorker(IServiceProvider serviceProvider, IEmailSender emailSender, ILogger<OrderCreateWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _emailSender = emailSender;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Worker OrderCreate Started");
                var factory = new ConnectionFactory()
                {
                    HostName = "localhost",
                    UserName = "guest",
                    Password = "guest",
                };

                var connection = await factory.CreateConnectionAsync();
                var channel = await connection.CreateChannelAsync();

                await channel.QueueDeclareAsync(queue: "order.created", durable: false, exclusive: false, autoDelete: false, arguments: null);

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (model, ea) =>
                {
                    try
                    {
                        using var scope = _serviceProvider.CreateScope();

                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        var orderCreated = JsonSerializer.Deserialize<OrderCreatedEvent>(message);

                        if (orderCreated is not null)
                        {
                            await _emailSender.SendEmailAsync(orderCreated.BuyerEmail, "Order Created", $"You ordered phone: {orderCreated.PhoneId}");
                            await _emailSender.SendEmailAsync(orderCreated.SellerEmail, "You have a buyer!", $"Someone bought your phone: {orderCreated.PhoneId}");
                        }
                        await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                    }

                };

                await channel.BasicConsumeAsync(queue: "order.created", autoAck: false, consumer: consumer);
                _logger.LogInformation("Worker OrderCreate Working...");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Worker OrderCreate Crashed {ex.Message}");
            }
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}

