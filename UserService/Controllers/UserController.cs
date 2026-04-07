using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using UserService.BLL.Abstractions;
using UserService.BLL.Models;
using UserService.Extentions;

namespace UserService.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;
        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id)
        {
            _logger.LogInformation("Getting User {Id}", id);

            var result = await _userService.GetById(id);

            return result.StatusCode switch
            {
                HttpStatusCode.OK => Ok(result.Body),
                HttpStatusCode.NotFound => NotFound(result.Message),
                HttpStatusCode.BadRequest => BadRequest(result.Message),
                HttpStatusCode.Forbidden => Forbid(result.Message),
                HttpStatusCode.InternalServerError => StatusCode(500, result.Message),
                _ => StatusCode(500, "Unexpected status code")
            };
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetMe()
        {
            var userId = HttpContext.GetUserId();
            _logger.LogInformation("Getting current User {UserId}", userId);

            var result = await _userService.GetById(userId);

            return result.StatusCode switch
            {
                HttpStatusCode.OK => Ok(result.Body),
                HttpStatusCode.NotFound => NotFound(result.Message),
                HttpStatusCode.BadRequest => BadRequest(result.Message),
                HttpStatusCode.Forbidden => Forbid(result.Message),
                HttpStatusCode.InternalServerError => StatusCode(500, result.Message),
                _ => StatusCode(500, "Unexpected status code")
            };
        }

        [HttpPatch]
        [Authorize]
        public async Task<IActionResult> Update([FromBody] UserUpdateDTO userUpdateDTO)
        {
            var userId = HttpContext.GetUserId();
            _logger.LogInformation("Updating User {UserId}", userId);

            var result = await _userService.UpdateAsync(userId, userUpdateDTO);

            return result.StatusCode switch
            {
                HttpStatusCode.OK => Ok(result.Message),
                HttpStatusCode.BadRequest => BadRequest(result.Message),
                HttpStatusCode.Forbidden => Forbid(result.Message),
                HttpStatusCode.InternalServerError => StatusCode(500, result.Message),
                _ => StatusCode(500, "Unexpected status code")
            };
        }
    }
}
