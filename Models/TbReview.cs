using System;
using System.Collections.Generic;

namespace vizin.Models;

public partial class TbReview
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid BookingId { get; set; }

    public int Note { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual TbBooking Booking { get; set; } = null!;

    public virtual TbUser User { get; set; } = null!;
}
