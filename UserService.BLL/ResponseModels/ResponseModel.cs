using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace UserService.BLL.ResponseModels
{
    public class ResponseModel
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; } =string.Empty;
    }
}
