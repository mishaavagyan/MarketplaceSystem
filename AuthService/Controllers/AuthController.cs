using AuthService.BLL.Abstractions;
using AuthService.BLL.Models;
using AuthService.BLL.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] UserDTO userDTO)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            _logger.LogInformation("Starting Registration for {Email} from IP {IP}", userDTO.Email, ip);

            var result = await _authService.RegisterAsync(userDTO);

            if (result.StatusCode == HttpStatusCode.BadRequest)
            {
                _logger.LogWarning("Registration Failed (BadRequest) for {Email}, {Message}", userDTO.Email, result.Message);
                return BadRequest(result.Message);
            }

            _logger.LogInformation("Successfully Registered User {Email}", userDTO.Email);
            return Ok(result.Message);
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmail([FromQuery] string email, [FromQuery] string code)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            _logger.LogInformation("Starting Verify Email for {Email} from IP {IP}", email, ip);

            var result = await _authService.VerifyEmailAsync(email, code);

            if (result.StatusCode == HttpStatusCode.BadRequest)
            {
                _logger.LogWarning("Verify Failed (BadRequest) for {Email}, {Message}", email, result.Message);
                return BadRequest(result.Message);
            }

            _logger.LogInformation("Successfully Verified Email for {Email}", email);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserDTO userDTO)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            _logger.LogInformation("Starting Login for {Email} from IP {IP}", userDTO.Email, ip);

            var result = await _authService.LoginAsync(userDTO);

            if (result.StatusCode == HttpStatusCode.BadRequest)
            {
                _logger.LogWarning("Login Failed (BadRequest) for {Email}, {Message}", userDTO.Email, result.Message);
                return BadRequest(result.Message);
            }

            _logger.LogInformation("Login Successful for {Email}, {Message}", userDTO.Email, result.Message);
            return Ok(result);
        }
    }
}
