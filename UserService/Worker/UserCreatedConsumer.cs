using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using UserService.BLL.Abstractions;
using UserService.BLL.Models;

namespace UserService.Worker
{
    public class UserCreatedConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<UserCreatedConsumer> _logger;

        public UserCreatedConsumer(IServiceProvider serviceProvider, ILogger<UserCreatedConsumer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = "localhost",
                    UserName = "guest",
                    Password = "guest"
                };

                var connection = await factory.CreateConnectionAsync();
                var channel = await connection.CreateChannelAsync();

                await channel.QueueDeclareAsync(
                    queue: "user.created",
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                var consumer = new AsyncEventingBasicConsumer(channel);

                consumer.ReceivedAsync += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    _logger.LogInformation("Received message from 'user.created' queue: {Message}", message);

                    try
                    {
                        var userCreated = JsonSerializer.Deserialize<UserCreatedEvent>(message);

                        if (userCreated == null)
                        {
                            _logger.LogWarning("Deserialized message is null. Raw message: {Message}", message);
                            await channel.BasicNackAsync(ea.DeliveryTag, false, false);
                            return;
                        }

                        using var scope = _serviceProvider.CreateScope();
                        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                        await userService.CreateAsync(userCreated);
                        _logger.LogInformation("Successfully created user profile for {UserId}", userCreated.UserId);

                        await channel.BasicAckAsync(ea.DeliveryTag, false);
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.LogError(jsonEx, "JSON deserialization failed for message: {Message}", message);
                        await channel.BasicNackAsync(ea.DeliveryTag, false, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unhandled error processing message: {Message}", message);
                        await channel.BasicNackAsync(ea.DeliveryTag, false, true);
                    }
                };

                await channel.BasicConsumeAsync(
                    queue: "user.created",
                    autoAck: false,
                    consumer: consumer
                );

                _logger.LogInformation("Worker is listening to 'user.created' queue.");
            }
            catch(Exception ex)
            {
                _logger.LogCritical(ex, "Failed To Run User Created Cunsumer");
            }
            
        }

    }

}
