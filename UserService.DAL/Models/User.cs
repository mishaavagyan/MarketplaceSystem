using System;
using System.Collections.Generic;

namespace UserService.DAL.Models;

public partial class User
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public double? Rating { get; set; }

    public string? Role { get; set; }

    public DateTime? CreatedAt { get; set; }
}
