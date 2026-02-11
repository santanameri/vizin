using System;
using System.Collections.Generic;

namespace vizin.Models;

public partial class TbAmenity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public int Category { get; set; }

    public virtual ICollection<TbProperty> Properties { get; set; } = new List<TbProperty>();
}
