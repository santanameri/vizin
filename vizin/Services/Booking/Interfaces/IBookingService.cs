using vizin.DTO.Booking;
using vizin.Models;

namespace vizin.Services.Booking.Interfaces;

public interface IBookingService
{
    Task<BookingResponseDto> CreateBooking(Guid userId, Guid propertyId, CreateBookingDto dto);
    Task<BookingHistoryDto> GetUserBookingHistoryAsync(Guid userId, string role);
    Task<bool> CancelBookingAsync(Guid bookingId, Guid userId);
    Task<byte[]> GenerateHostReportAsync(Guid hostId);
}