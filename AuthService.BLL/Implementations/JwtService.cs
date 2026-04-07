using AuthService.BLL.Abstractions;
using AuthService.BLL.Models;
using AuthService.DAL.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthService.BLL.Implementations
{
    public class JwtService : IJwtService
    {
        private readonly AuthSettings _options;
        private readonly ILogger<JwtService> _logger;
        public JwtService(IOptions<AuthSettings> options,ILogger<JwtService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }
        public string GenerateToken(User user)
        {
            try
            {
                _logger.LogInformation("Starting Generate JWT Token For {Email}",user.Email);
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email)
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _options.Issuer,
                    audience: _options.Audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(1),
                    signingCredentials: creds);
                _logger.LogInformation("Successfully created JWT token for {Email}",user.Email);
                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed To Generate Token For {Email}", user.Email);
                throw;
            }
            
        }
    }
}
