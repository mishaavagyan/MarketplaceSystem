using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService.Models
{
    public class OrderUpdatedEvent
    {
        public Guid Id { get; set; }
        public int Status { get; set; } 
        public DateTime UpdatedAt { get; set; }
        public string BuyerEmail { get; set; } = string.Empty;
        public string SellerEmail { get; set; } = string.Empty;
    }
}
