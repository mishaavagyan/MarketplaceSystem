using OrderService.Extensions;

namespace OrderService.Middlewares
{
    public class ExceptionHandleMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandleMiddleware> _logger;

        public ExceptionHandleMiddleware(RequestDelegate next, ILogger<ExceptionHandleMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var userId = context.GetUserId();

                _logger.LogError(ex, "Unhandled exception for User {UserId}", userId);

                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    Message = "Internal server error occurred."
                };

                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }
}
