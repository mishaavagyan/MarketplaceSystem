using AuthService.BLL.Abstractions;
using AuthService.BLL.Models;
using AuthService.BLL.ResponseModels;
using AuthService.DAL.Abstractions;
using AuthService.DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.RegularExpressions;

namespace AuthService.BLL.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _context;
        private readonly IJwtService _jwtService;
        private readonly IRabbitMQPublisher _rabbitMQPublisher;
        private readonly IRedisService _redis;
        private readonly ILogger<AuthService> _logger;
        private readonly IEmailSenderService _emailSenderService;
        public AuthService(IUnitOfWork context, IJwtService jwtService, IRabbitMQPublisher rabbitMQPublisher, IRedisService redis, ILogger<AuthService> logger, IEmailSenderService emailSenderService)
        {
            _context = context;
            _jwtService = jwtService;
            _rabbitMQPublisher = rabbitMQPublisher;
            _redis = redis;
            _logger = logger;
            _emailSenderService = emailSenderService;
        }

        public async Task<ResponseModel> VerifyEmailAsync(string email, string code)
        {
            string? storedCode = null;

            try
            {
                storedCode = await _redis.GetVerificationCodeAsync(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting verification code from Redis for {Email}", email);
                return new ResponseModel
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "Internal server error while reading verification code"
                };
            }

            if (storedCode is null || storedCode != code)
            {
                _logger.LogWarning("Incorrect or expired verification code for {Email}", email);
                return new ResponseModel
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "Incorrect or expired verification code"
                };
            }

            _logger.LogInformation("Verification code is correct for {Email}", email);

            try
            {
                var user = await _context.Users.GetAsync(x => x.Email == email);
                if (user is null)
                {
                    _logger.LogWarning("User not found for email verification: {Email}", email);
                    return new ResponseModel
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Message = "User not found"
                    };
                }

                user.IsEmailConfirmed = true;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Email confirmed and user updated: {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user email confirmation in DB: {Email}", email);
                return new ResponseModel
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "Database error while confirming email"
                };
            }

            try
            {
                await _redis.DeleteVerificationCodeAsync(email);
                _logger.LogInformation("Verification code deleted from Redis for {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting verification code from Redis for {Email}", email);
            }

            return new ResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Email successfully confirmed"
            };
        }

        public async Task<ResponseModelJwt> LoginAsync(UserDTO userDTO)
        {
            if (String.IsNullOrEmpty(userDTO.Email) || String.IsNullOrEmpty(userDTO.Password))
            {
                return new ResponseModelJwt { Message = "Email or Password Are Required", StatusCode = HttpStatusCode.BadRequest };
            }
            if (userDTO.Email.Length < 10 || userDTO.Email.Length > 60)
            {
                return new ResponseModelJwt { Message = "Invalid Email Lenght.", StatusCode = HttpStatusCode.BadRequest };
            }
            if (!Regex.IsMatch(userDTO.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                return new ResponseModelJwt { Message = "Invalid Email Syntax.", StatusCode = HttpStatusCode.BadRequest };
            }
            if (userDTO.Password.Length < 8 || userDTO.Password.Length > 14)
            {
                return new ResponseModelJwt { Message = "Invalid Password Length", StatusCode = HttpStatusCode.BadRequest };
            }

            try
            {
                var user = await _context.Users.GetAsync(x => x.Email == userDTO.Email);
                if (user is not null)
                {
                    var password = new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, userDTO.Password);
                    if (password == PasswordVerificationResult.Success)
                    {
                        var token = _jwtService.GenerateToken(user);
                        _logger.LogInformation("Login Successfull for {Email}", userDTO.Email);
                        return new ResponseModelJwt { Message = "Success", StatusCode = HttpStatusCode.OK, JWT = token };
                    }
                }
                _logger.LogWarning("Incorrect Email or Password: {Email}", userDTO.Email);
                return new ResponseModelJwt { StatusCode = HttpStatusCode.BadRequest, Message = "Incorrect Email or Password" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception during login for {Email}", userDTO.Email);
                return new ResponseModelJwt { Message = "Internal Server Error", StatusCode = HttpStatusCode.InternalServerError };
            }
        }

        public async Task<ResponseModel> RegisterAsync(UserDTO userDTO)
        {
            if (String.IsNullOrEmpty(userDTO.Email) || String.IsNullOrEmpty(userDTO.Password))
            {
                return new ResponseModel { Message = "Email or Password Are Required", StatusCode = HttpStatusCode.BadRequest };
            }
            if (userDTO.Email.Length < 15 || userDTO.Email.Length > 40)
            {
                return new ResponseModel { Message = "Invalid Email Lenght.", StatusCode = HttpStatusCode.BadRequest };
            }
            if (!Regex.IsMatch(userDTO.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                return new ResponseModel { Message = "Invalid Email Syntax.", StatusCode = HttpStatusCode.BadRequest };
            }
            if (userDTO.Password.Length < 8 || userDTO.Password.Length > 15)
            {
                return new ResponseModel { Message = "Invalid Password Length", StatusCode = HttpStatusCode.BadRequest };
            }
            var existingUser = await _context.Users.GetAsync(x => x.Email == userDTO.Email);
            if (existingUser is not null)
            {
                return new ResponseModel { StatusCode = HttpStatusCode.BadRequest, Message = "Email Is Already In Use" };
            }
            var password = new PasswordHasher<UserDTO>().HashPassword(userDTO, userDTO.Password);
            User user = new User
            {
                Email = userDTO.Email,
                PasswordHash = password
            };
            try
            {
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User saved to DB Id: {UserId}, Email: {Email}", user.Id, user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving user to database: {Email}", userDTO.Email);
                return new ResponseModel { Message = "DataBase Error", StatusCode = HttpStatusCode.InternalServerError };
            }

            try
            {
                var userCreatedEvent = new UserCreatedEvent
                {
                    Email = user.Email,
                    UserId = user.Id,
                };

                await _rabbitMQPublisher.PublishUserCreated(userCreatedEvent);

                _logger.LogInformation("Published UserCreatedEvent to RabbitMQ UserId: {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while publishing UserCreatedEvent to RabbitMQ for {Email}", user.Email);
            }

            try
            {
                await _emailSenderService.SendEmail(user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification email to {Email}", user.Email);
            }

            _logger.LogInformation("Verification email sent to {Email}", user.Email);
            return new ResponseModel { Message = "Registred Succcessfully", StatusCode = HttpStatusCode.OK };

        }


    }
}
