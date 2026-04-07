using Microsoft.AspNetCore.Mvc;
using OrderService.BLL.Models.ResponseModel;
using System.Net;

namespace OrderService.BLL.Implementations
{
    public static class ControllerExtensions
    {
        public static IActionResult HandleResponse<T>(this ControllerBase controller, ResponseWithBody<T> result)
        {
            return result.StatusCode switch
            {
                HttpStatusCode.BadRequest => controller.BadRequest(result.Message),
                HttpStatusCode.NotFound => controller.NotFound(result.Message),
                HttpStatusCode.Forbidden => controller.Forbid(),
                HttpStatusCode.InternalServerError => controller.StatusCode(500, result.Message),
                HttpStatusCode.ServiceUnavailable => controller.StatusCode(503, result.Message),
                _ => controller.Ok(result)
            };
        }
    }
}
