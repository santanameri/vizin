using vizin.Models;

namespace vizin.Repositories.Booking.Interfaces;

public interface IBookingRepository
{
    Task<TbProperty?> GetPropertyWithCapacityAsync(Guid propertyId);
    Task<bool> HasConflictingBookingAsync(Guid propertyId, DateTime checkIn, DateTime checkOut);
    Task<TbBooking> CreateAsync(TbBooking booking);
}