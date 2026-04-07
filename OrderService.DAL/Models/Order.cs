using System;
using System.Collections.Generic;

namespace OrderService.DAL.Models;

public partial class Order
{
    public Guid Id { get; set; }

    public Guid BuyerId { get; set; }

    public Guid SellerId { get; set; }

    public Guid PhoneId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int Status { get; set; }
}
