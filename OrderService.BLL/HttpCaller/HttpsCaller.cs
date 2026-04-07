using Microsoft.AspNetCore.Http;
using OrderService.BLL.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OrderService.BLL.HttpCaller
{
    public class HttpsCaller
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpsCaller(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {   
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClient;
        }

        public async Task<PhoneDTO?> GetPhoneByIdAsync(Guid phoneId)
        {
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(token))
                return null;

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://localhost:7125/Phone/GetById?id={phoneId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return null;    
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PhoneDTO>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        public async Task<UserDTO?> GetUserByIdAsync(Guid userId)   
        {   
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(token))
                return null;

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://localhost:7095/User/GetById?id={userId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UserDTO>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}
