using vizin.Models;

namespace vizin.Repositories.Booking.Interfaces;

public interface IBookingRepository
{
    Task<TbProperty?> GetPropertyWithCapacityAsync(Guid propertyId);
    Task<bool> HasConflictingBookingAsync(Guid propertyId, DateTime checkIn, DateTime checkOut);
    Task<TbBooking> CreateAsync(TbBooking booking);
    Task<List<TbBooking>> GetHostBookingsAsync(Guid hostId);
    Task<List<TbBooking>> GetGuestBookingsAsync(Guid guestId);
    Task<TbBooking?> GetByIdAsync(Guid id);
    Task UpdateAsync(TbBooking booking);
    Task<IEnumerable<TbBooking>> GetBookingsByHostIdAsync(Guid hostId);
}