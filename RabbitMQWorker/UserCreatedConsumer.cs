using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using UserService.BLL.Abstractions;
using UserService.BLL.Models;

namespace RabbitMQWorker
{
    public class UserCreatedConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public UserCreatedConsumer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine(1);
            var factory = new ConnectionFactory() { HostName = "localhost",UserName="guest",Password = "guest" };
            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync("user.created", false, false, false, null);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                using var scope = _serviceProvider.CreateScope();
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var userCreated = JsonSerializer.Deserialize<UserCreatedEvent>(json);

                if (userCreated is not null)
                {
                    await userService.CreateAsync(new UserCreatedEvent
                    {
                        UserId = userCreated.UserId,
                        Email = userCreated.Email
                    });
                }
            };

            await channel.BasicConsumeAsync("user.created", true, consumer);


        }
    }

}
