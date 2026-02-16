using vizin.DTO.Booking;
using vizin.Models;

namespace vizin.Services.Booking.Interfaces;

public interface IBookingService
{
    Task<BookingResponseDto> CreateBooking(Guid userId, Guid propertyId, CreateBookingDto dto);
}