using System;
using System.Collections.Generic;

namespace vizin.Models;

public partial class TbBooking
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid PropertyId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int Status { get; set; }

    public DateTime CheckinDate { get; set; }

    public DateTime CheckoutDate { get; set; }

    public DateTime? CancelationDate { get; set; }

    public int GuestCount { get; set; }

    public virtual TbProperty Property { get; set; } = null!;

    public virtual ICollection<TbPayment> TbPayments { get; set; } = new List<TbPayment>();

    public virtual ICollection<TbReview> TbReviews { get; set; } = new List<TbReview>();

    public virtual TbUser User { get; set; } = null!;
}
