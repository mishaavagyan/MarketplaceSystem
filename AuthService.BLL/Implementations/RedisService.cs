using AuthService.BLL.Abstractions;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace AuthService.BLL.Implementations
{
    public class RedisService : IRedisService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;
        private readonly ILogger<RedisService> _logger;

        public RedisService(IConnectionMultiplexer redis, ILogger<RedisService> logger)
        {
            _redis = redis;
            _db = redis.GetDatabase();
            _logger = logger;
        }

        public async Task SetVerificationCodeAsync(string email, string code, TimeSpan expiration)
        {
            try
            {
                _logger.LogInformation("Starting Set Verfication Code For {Email}",email);
                await _db.StringSetAsync($"verify:{email}", code, expiration);
                _logger.LogInformation("Successfully Set To Redis Verfication Code for {Email}",email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed To Set Code For Verfication For {Email}", email);
                throw;
            }
        }

        public async Task<string?> GetVerificationCodeAsync(string email)
        {
            try
            {
                _logger.LogInformation("Getting Verification Code for {Email}",email);
                var code = await _db.StringGetAsync($"verify:{email}");
                _logger.LogInformation("Retrived Verification code for {Email}",email);
                return code;
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Error while getting verification code for {Email}",email);
                throw;
            }
        }

        public async Task DeleteVerificationCodeAsync(string email)
        {
            try
            {
                _logger.LogInformation("Starting Delete Verification Code for {Email}",email);
                await _db.KeyDeleteAsync($"verify:{email}");
                _logger.LogInformation("Successfully Deleted Verification Code for {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Error while Deleting verification code for {Email}", email);
                throw;
            }
        }
    }

}
