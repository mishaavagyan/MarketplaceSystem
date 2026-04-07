using Microsoft.AspNetCore.Mvc;
using PhoneService.BLL.Abstractions;
using PhoneService.BLL.Models.DTO;
using PhoneService.BLL.Models.ResponseModels;
using PhoneService.Extentions;
using System.Net;

namespace PhoneService.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class PhoneController : ControllerBase
    {
        private readonly IPhoneService _phoneService;
        private readonly ILogger<PhoneController> _logger;
        public PhoneController(IPhoneService phoneService, ILogger<PhoneController> logger)
        {
            _phoneService = phoneService;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetById([FromQuery] Guid id)
        {
            _logger.LogInformation("Getting phone {PhoneId}", id);

            var result = await _phoneService.GetAsync(id);

            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Phone not found {PhoneId}", id);
                return NotFound(result.Message);
            }

            return Ok(result.Body);
        }
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] FiltrModel filtrModel)
        {
            _logger.LogInformation("Getting phones list");

            var result = await _phoneService.GetAllAsync(filtrModel);

            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("No phones found");
                return NotFound(result.Message);
            }

            return Ok(result.Body);
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PhoneCreateDTO phoneCreate)
        {
            var userId = HttpContext.GetUserId();

            _logger.LogInformation("Creating phone for user {UserId}", userId);

            var result = await _phoneService.CreateAsync(userId, phoneCreate);

            if (result.StatusCode == HttpStatusCode.BadRequest)
            {
                _logger.LogWarning("Create failed for {UserId}: {Message}", userId, result.Message);
                return BadRequest(result.Message);
            }

            return Ok(result.Message);
        }
        [HttpDelete]
        public async Task<IActionResult> Remove([FromQuery] Guid productId)
        {
            var userId = HttpContext.GetUserId();

            _logger.LogInformation("Deleting phone {ProductId} for {UserId}", productId, userId);

            var result = await _phoneService.RemoveAsync(productId, userId);

            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Phone not found {ProductId}", productId);
                return NotFound(result.Message);
            }

            if (result.StatusCode == HttpStatusCode.Forbidden)
            {
                _logger.LogWarning("Forbidden delete attempt {ProductId}", productId);
                return Forbid();
            }

            return Ok(result.Message);
        }
        [HttpPut]
        public async Task<IActionResult> Update([FromQuery] Guid productId, [FromBody] PhoneUpdateDTO phoneUpdate)
        {
            var userId = HttpContext.GetUserId();

            _logger.LogInformation("Updating phone {ProductId} for {UserId}", productId, userId);

            var result = await _phoneService.UpdateAsync(productId, userId, phoneUpdate);

            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Phone not found {ProductId}", productId);
                return NotFound(result.Message);
            }

            if (result.StatusCode == HttpStatusCode.Forbidden)
            {
                _logger.LogWarning("Forbidden update attempt {ProductId}", productId);
                return Forbid();
            }

            if (result.StatusCode == HttpStatusCode.BadRequest)
            {
                _logger.LogWarning("Update failed {ProductId}: {Message}", productId, result.Message);
                return BadRequest(result.Message);
            }

            return Ok(result.Message);
        }
    }
}
