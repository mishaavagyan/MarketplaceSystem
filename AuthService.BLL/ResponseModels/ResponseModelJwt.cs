using System.Net;

namespace AuthService.BLL.ResponseModels
{
    public class ResponseModelJwt
    {
        public string Message { get; set; } = string.Empty;
        public HttpStatusCode StatusCode { get; set; }
        public string JWT { get; set; } = string.Empty;
    }
}
