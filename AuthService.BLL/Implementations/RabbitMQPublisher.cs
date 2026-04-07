using AuthService.BLL.Abstractions;
using AuthService.BLL.Models;
using Microsoft.Extensions.Configuration;

namespace AuthService.BLL.Implementations
{
    using Microsoft.Extensions.Logging;
    using RabbitMQ.Client;
    using System.Text;
    using System.Text.Json;

    public class RabbitMQPublisher : IRabbitMQPublisher
    {
        private readonly IConfiguration _config;
        private readonly ILogger<RabbitMQPublisher> _logger;

        public RabbitMQPublisher(IConfiguration config, ILogger<RabbitMQPublisher> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task PublishUserCreated(UserCreatedEvent userCreated)
        {
            string? queue = "user.created";

            try
            {
                _logger.LogInformation("Publishing Messege to queue {Queue}", queue);

                var factory = new ConnectionFactory
                {
                    HostName = "localhost",
                    UserName = "guest",
                    Password = "guest"
                };

                await using var connection = await factory.CreateConnectionAsync();
                await using var channel = await connection.CreateChannelAsync();

                await channel.QueueDeclareAsync(
                    queue: "user.created",
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var json = JsonSerializer.Serialize(userCreated);
                var body = Encoding.UTF8.GetBytes(json);

                await channel.BasicPublishAsync(
                    exchange: "",
                    routingKey: "user.created",
                    body: body);
                _logger.LogInformation("Successfully Published to queue {Queue}", queue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed To Publish Message to queue {Queue}", queue);
                throw;
            }

        }
    }

}