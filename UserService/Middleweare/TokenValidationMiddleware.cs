using System.Security.Claims;
using System.Text.Json;

namespace UserService.Middleweare
{
    public class TokenValidationMiddleware 
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TokenValidationMiddleware> _logger;
            
        public TokenValidationMiddleware(RequestDelegate next, ILogger<TokenValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                _logger.LogInformation("Starting Get UserId from JWT");
                var userIdString = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!Guid.TryParse(userIdString, out var userId))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonSerializer.Serialize(new
                    {
                        Message = "Invalid Token"
                    }));
                    return;
                }

                context.Items["UserId"] = userId;
                _logger.LogInformation("Successfully Geted Id from JWT for {userId}",userId);
                await _next(context);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Error Can't Get Id from JWT");
            }
            
        }
    }
}
