namespace AuthService.BLL.Abstractions
{
    public interface IRedisService
    {
        public  Task SetVerificationCodeAsync(string email, string code, TimeSpan expiration);
        public  Task<string?> GetVerificationCodeAsync(string email);
        public  Task DeleteVerificationCodeAsync(string email);
    }
}
