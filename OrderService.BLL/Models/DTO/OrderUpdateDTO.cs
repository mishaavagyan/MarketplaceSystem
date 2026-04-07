using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.BLL.Models.DTO
{
    public class OrderUpdateDTO
    {
        public Guid Id { get; set; }
        public OrderStatus Status { get; set; }
    }
    public enum OrderStatus
    {
        Pending,
        Confirmed,
        Cancelled,
    }
}
