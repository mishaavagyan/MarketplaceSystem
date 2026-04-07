using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using PhoneService.BLL.Abstractions;
using PhoneService.BLL.Models.DTO;
using PhoneService.BLL.Models.ResponseModels;
using PhoneService.DLL.Abstractions;
using PhoneService.DLL.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PhoneService.BLL.Implementations
{
    public class PhoneService : IPhoneService
    {
        private readonly IUnitOfWork _context;
        private readonly ILogger<PhoneService> _logger; 
        public PhoneService(IUnitOfWork context, ILogger<PhoneService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<ResponseModel> CreateAsync(Guid userId, PhoneCreateDTO phoneCreate)
        {
            _logger.LogInformation("Creating phone for user {UserId}", userId);

            var isValid = ProductValidation(phoneCreate, out string message);
            if (!isValid)
                return new ResponseModel { Message = message, StatusCode = HttpStatusCode.BadRequest };

            var phone = new Phone
            {
                Id = Guid.NewGuid(),
                Price = phoneCreate.Price,
                Brand = phoneCreate.Brand,
                Description = phoneCreate.Description,
                Model = phoneCreate.Model,
                OwnerId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Phones.AddAsync(phone);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Phone created successfully for user {UserId}", userId);

            return new ResponseModel
            {
                Message = "Created Successfully",
                StatusCode = HttpStatusCode.OK
            };
        }

        public async Task<ResponseModelWithBody<IEnumerable<PhoneViewDTO>>> GetAllAsync(FiltrModel filtrModel)
        {
            _logger.LogInformation("Getting phones with filters");

            var query = _context.Phones.GetAllQueryable();

            if (!string.IsNullOrWhiteSpace(filtrModel.Model) && filtrModel.Model != "string")
                query = query.Where(x => x.Model == filtrModel.Model);

            if (filtrModel.minPrice > 0 && filtrModel.maxPrice > filtrModel.minPrice)
                query = query.Where(x => x.Price >= filtrModel.minPrice && x.Price <= filtrModel.maxPrice);

            if (!string.IsNullOrWhiteSpace(filtrModel.Description) && filtrModel.Description != "string")
                query = query.Where(x => x.Description == filtrModel.Description);

            if (!string.IsNullOrWhiteSpace(filtrModel.Brand) && filtrModel.Brand != "string")
                query = query.Where(x => x.Brand == filtrModel.Brand);

            var result = await query
                .Take(100)
                .Select(x => new PhoneViewDTO
                {
                    Id = x.Id,
                    Brand = x.Brand,
                    Model = x.Model,
                    Description = x.Description,
                    Price = x.Price,
                    CreatedAt = x.CreatedAt,
                    OwnerId = x.OwnerId
                })
                .ToListAsync();

            if (!result.Any())
            {
                return new ResponseModelWithBody<IEnumerable<PhoneViewDTO>>
                {
                    Message = "No products found",
                    StatusCode = HttpStatusCode.NotFound
                };
            }

            return new ResponseModelWithBody<IEnumerable<PhoneViewDTO>>
            {
                Body = result,
                Message = "Success",
                StatusCode = HttpStatusCode.OK
            };
        }

        public async Task<ResponseModelWithBody<PhoneViewDTO>> GetAsync(Guid productId)
        {
            _logger.LogInformation("Getting phone {ProductId}", productId);

            var product = await _context.Phones.GetAsync(x => x.Id == productId);

            if (product is null)
                return new ResponseModelWithBody<PhoneViewDTO>
                {
                    Message = "Not Found",
                    StatusCode = HttpStatusCode.NotFound
                };

            var dto = new PhoneViewDTO
            {
                Id = product.Id,
                Brand = product.Brand,
                Model = product.Model,
                Description = product.Description,
                Price = product.Price,
                CreatedAt = product.CreatedAt,
                OwnerId = product.OwnerId
            };

            return new ResponseModelWithBody<PhoneViewDTO>
            {
                Body = dto,
                Message = "Success",
                StatusCode = HttpStatusCode.OK
            };
        }

        public async Task<ResponseModel> RemoveAsync(Guid productId, Guid userId)
        {
            _logger.LogInformation("Deleting phone {ProductId}", productId);

            var product = await _context.Phones.GetAsync(x => x.Id == productId);

            if (product is null)
                return new ResponseModel { Message = "Not Found", StatusCode = HttpStatusCode.NotFound };

            if (product.OwnerId != userId)
                return new ResponseModel { Message = "Forbidden", StatusCode = HttpStatusCode.Forbidden };

            _context.Phones.Delete(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Phone deleted {ProductId}", productId);

            return new ResponseModel
            {
                Message = "Deleted Successfully",
                StatusCode = HttpStatusCode.OK
            };
        }

        public async Task<ResponseModel> UpdateAsync(Guid productId, Guid userId, PhoneUpdateDTO phoneUpdate)
        {
            _logger.LogInformation("Updating phone {ProductId}", productId);

            var product = await _context.Phones.GetAsync(x => x.Id == productId);

            if (product is null)
                return new ResponseModel { Message = "Not Found", StatusCode = HttpStatusCode.NotFound };

            if (product.OwnerId != userId)
                return new ResponseModel { Message = "Forbidden", StatusCode = HttpStatusCode.Forbidden };

            var isValid = ProductValidation(phoneUpdate, out string message);
            if (!isValid)
                return new ResponseModel { Message = message, StatusCode = HttpStatusCode.BadRequest };

            product.Description = phoneUpdate.Description;
            product.Brand = phoneUpdate.Brand;
            product.Price = phoneUpdate.Price;
            product.Model = phoneUpdate.Model;

            _context.Phones.Update(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Phone updated {ProductId}", productId);

            return new ResponseModel
            {
                Message = "Updated Successfully",
                StatusCode = HttpStatusCode.OK
            };
        }
        public bool ProductValidation(PhoneCreateDTO product, out string message)
        {
            if (string.IsNullOrWhiteSpace(product.Model) || product.Model.Length < 3 || product.Model.Length > 15)
            {
                message = "Model must be between 3 and 15 characters";
                return false;
            }

            if (string.IsNullOrWhiteSpace(product.Brand) || product.Brand.Length < 3 || product.Brand.Length > 15)
            {
                message = "Brand must be between 3 and 15 characters";
                return false;
            }

            if (string.IsNullOrWhiteSpace(product.Description) || product.Description.Length < 5 || product.Description.Length > 40)
            {
                message = "Description must be between 5 and 40 characters";
                return false;
            }

            if (product.Price < 1 || product.Price > 1000000)
            {
                message = "Price must be between 1 and 1,000,000";
                return false;
            }

            message = "Success";
            return true;
        }
    }
}
