using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.RegularExpressions;
using UserService.BLL.Abstractions;
using UserService.BLL.Models;
using UserService.BLL.ResponseModels;
using UserService.DAL.Abstractions;
using UserService.DAL.Models;

namespace UserService.BLL.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _context;
        private readonly ILogger<UserService> _logger;
        public UserService(IUnitOfWork context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<ResponseModel> CreateAsync(UserCreatedEvent user)
        {
            _logger.LogInformation("Starting Create User Profile {Email}", user.Email);

            try
            {
                var newUser = new User
                {
                    Id = user.UserId,
                    Email = user.Email,
                    Name = "",
                    Phone = "",
                    Role = "User"
                };

                await _context.Users.CreateAsync(newUser);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully Created User Profile {Email}", user.Email);
                return new ResponseModel
                {
                    Message = "Created Successfully",
                    StatusCode = HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred While Creating User Profile {Email}", user.Email);
                return new ResponseModel
                {
                    Message = "Internal Server Error",
                    StatusCode = HttpStatusCode.InternalServerError
                };
            }
        }

        public async Task<ResponseModelWithBody<UserViewDTO>> GetById(Guid id)
        {
            _logger.LogInformation("Starting Get User By Id {Id}", id);

            try
            {
                if (id == Guid.Empty)
                    return new ResponseModelWithBody<UserViewDTO>
                    {
                        Message = "Invalid Id",
                        StatusCode = HttpStatusCode.BadRequest
                    };

                var user = await _context.Users.GetAsync(x => x.Id == id);
                if (user is null)
                {
                    _logger.LogWarning("User Not Found {Id}", id);
                    return new ResponseModelWithBody<UserViewDTO>
                    {
                        Message = "User Not Found",
                        StatusCode = HttpStatusCode.NotFound
                    };
                }

                var dto = new UserViewDTO
                {
                    Email = user.Email,
                    Name = user.Name,
                    Phone = user.Phone,
                    Rating = user.Rating
                };

                _logger.LogInformation("Successfully Retrieved User {Id}", id);
                return new ResponseModelWithBody<UserViewDTO>
                {
                    Body = dto,
                    Message = "Success",
                    StatusCode = HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred While Getting User By Id {Id}", id);
                return new ResponseModelWithBody<UserViewDTO>
                {
                    Body = null,
                    Message = "Internal Server Error",
                    StatusCode = HttpStatusCode.InternalServerError
                };
            }
        }

        public async Task<ResponseModel> UpdateAsync(Guid id, UserUpdateDTO userUpdate)
        {
            _logger.LogInformation("Starting Update User {Id}", id);

            try
            {
                if (id == Guid.Empty)
                    return new ResponseModel { Message = "Invalid Id", StatusCode = HttpStatusCode.BadRequest };

                if (string.IsNullOrWhiteSpace(userUpdate.Name) || userUpdate.Name.Length < 3 || userUpdate.Name.Length > 30)
                    return new ResponseModel { Message = "Invalid Name Length", StatusCode = HttpStatusCode.BadRequest };

                if (String.IsNullOrEmpty(userUpdate.Phone) || !IsArmenianPhoneNumber(userUpdate.Phone))
                    return new ResponseModel { Message = "Invalid Phone Number", StatusCode = HttpStatusCode.BadRequest };

                var userForUpdate = await _context.Users.GetAsync(x => x.Id == id);
                if (userForUpdate is null)
                    return new ResponseModel { Message = "User Not Found", StatusCode = HttpStatusCode.NotFound };

                userForUpdate.Name = userUpdate.Name;
                userForUpdate.Phone = userUpdate.Phone;

                _context.Users.Update(userForUpdate);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User {Id} Successfully Updated", id);
                return new ResponseModel { Message = "Updated Successfully", StatusCode = HttpStatusCode.OK };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred While Updating User {Id}", id);
                return new ResponseModel { Message = "Internal Server Error", StatusCode = HttpStatusCode.InternalServerError };
            }
        }

        private static bool IsArmenianPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            phoneNumber = phoneNumber.Replace(" ", "").Replace("-", "");

            var pattern = @"^(?:\+374|00374)([1-9][0-9])\d{6}$";
            return Regex.IsMatch(phoneNumber, pattern);
        }
    }
}
