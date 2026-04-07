using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneService.BLL.Models.ResponseModels
{
    public class FiltrModel
    {
        public string? Brand { get; set; }
        public string? Model { get; set; }  
        public decimal? minPrice { get; set; }
        public decimal? maxPrice { get; set; }
        public string? Description { get; set; }
    }
}
