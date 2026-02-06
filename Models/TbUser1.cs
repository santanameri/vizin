using System;
using System.Collections.Generic;

namespace vizin.Models;

public partial class TbUser1
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Password { get; set; } = null!;
}
