using vizin.DTO.Booking;

namespace vizin.Services.Booking.Interfaces;

public interface IPaymentService
{
    Task<PaymentResponseDto> ProcessPaymentAsync(Guid bookingId, Guid userId, PaymentRequestDto dto);
}