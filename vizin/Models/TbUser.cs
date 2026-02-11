using System;
using System.Collections.Generic;

namespace vizin.Models;

public partial class TbUser
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int Type { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<TbBooking> TbBookings { get; set; } = new List<TbBooking>();

    public virtual ICollection<TbFavorite> TbFavorites { get; set; } = new List<TbFavorite>();

    public virtual ICollection<TbProperty> TbProperties { get; set; } = new List<TbProperty>();

    public virtual ICollection<TbReview> TbReviews { get; set; } = new List<TbReview>();
}
