using System;
using System.Collections.Generic;

namespace vizin.Models;

public partial class TbFavorite
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid PropertyId { get; set; }

    public virtual TbProperty Property { get; set; } = null!;

    public virtual TbUser User { get; set; } = null!;
}
