using NotificationService.Implementations;
using NotificationService.Interfaces;

namespace NotificationService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddHostedService<OrderUpdateWorker>();
            builder.Services.AddHostedService<OrderCreateWorker>();
            builder.Services.AddSingleton<IEmailSender, EmailSender>();
            var host = builder.Build();
            host.Run();
        }
    }
}