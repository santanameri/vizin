using System;
using System.Collections.Generic;

namespace vizin.Models;

public partial class TbProperty
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string FullAddress { get; set; } = null!;

    public bool Availability { get; set; }

    public decimal DailyValue { get; set; }

    public int Capacity { get; set; }

    public int AccomodationType { get; set; }

    public int PropertyCategory { get; set; }

    public Guid UserId { get; set; }

    public virtual ICollection<TbBooking> TbBookings { get; set; } = new List<TbBooking>();

    public virtual ICollection<TbFavorite> TbFavorites { get; set; } = new List<TbFavorite>();

    public virtual TbUser User { get; set; } = null!;

    public virtual ICollection<TbAmenity> Amenities { get; set; } = new List<TbAmenity>();
}
