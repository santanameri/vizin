using vizin.Models;

namespace vizin.Repositories.Booking.Interfaces;

public interface IPaymentRepository
{
    Task<List<TbPayment>> GetByBookingIdAsync(Guid bookingId);
    Task CreateAsync(TbPayment payment);
}