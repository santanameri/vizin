using System;
using System.Collections.Generic;
using vizin.Models.Enum;

namespace vizin.Models;

public partial class TbPayment
{
    public Guid Id { get; set; }

    public Guid BookingId { get; set; }

    public PaymentMethodType PaymentMethod { get; set; }

    public decimal Amount { get; set; }

    public int StatusPayment { get; set; }

    public DateTime? PaymentDate { get; set; }

    public int Type { get; set; }

    public virtual TbBooking Booking { get; set; } = null!;
}
