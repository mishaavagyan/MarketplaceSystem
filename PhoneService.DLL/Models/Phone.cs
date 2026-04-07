using System;
using System.Collections.Generic;

namespace PhoneService.DLL.Models;

public partial class Phone
{
    public Guid Id { get; set; }

    public string? Brand { get; set; }

    public string? Model { get; set; }

    public decimal? Price { get; set; }

    public string? Description { get; set; }

    public string? PhotoUrl { get; set; }

    public Guid OwnerId { get; set; }

    public DateTime? CreatedAt { get; set; }
}
