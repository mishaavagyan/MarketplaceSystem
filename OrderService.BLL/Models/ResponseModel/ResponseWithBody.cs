using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.BLL.Models.ResponseModel
{
    public class ResponseWithBody<T>
    {
        public string Message { get; set; } = string.Empty;
        public HttpStatusCode StatusCode { get; set; }  
        public T? Body { get; set; }
    }
}
