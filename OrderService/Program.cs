
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OrderService.BLL.Abstractions;
using OrderService.BLL.HttpCaller;
using OrderService.BLL.Implementations;
using OrderService.BLL.Models.DTO;
using OrderService.DAL.Abstractions;
using OrderService.DAL.Implementations;
using OrderService.DAL.Models;
using OrderService.Middlewares;
using Serilog;
using System.Text;

namespace OrderService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Host.UseSerilog((context, services, config) => config
          .WriteTo.Console()
          .WriteTo.File("Logs/order_log.txt", rollingInterval: RollingInterval.Day));

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHttpClient<HttpsCaller>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Введите JWT токен с 'Bearer ' префиксом. Пример: Bearer 12345abcdef",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id ="Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });


            });
            builder.Services.AddDbContext<OrderDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("ConnString")));
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IOrderService, BLL.Implementations.OrderService>();
            builder.Services.AddScoped<IRabbitMQPublisher, RabbitMQPublisher>();
            builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection("AuthSettings"));
            var authSettings = builder.Configuration.GetSection("AuthSettings").Get<AuthSettings>();
            builder.Services.AddAuthentication("Bearer").AddJwtBearer("Bearer", options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = authSettings?.Issuer,
                    ValidAudience = authSettings?.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authSettings?.SecretKey!))

                };
            });
            builder.Services.AddAuthorization();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<TokenValidationMiddleware>();
            app.UseMiddleware<ExceptionHandleMiddleware>();



            app.MapControllers();

            app.Run();
        }
    }
}
