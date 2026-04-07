using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.BLL.Models.DTO
{
    public class OrderCreatedEvent
    {
        public Guid Id { get; set; }
        public Guid BuyerId { get; set; }
        public Guid SellerId { get; set; }
        public Guid PhoneId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string BuyerEmail { get; set; } = string.Empty;
        public string SellerEmail { get; set; } = string.Empty;
    }
}
