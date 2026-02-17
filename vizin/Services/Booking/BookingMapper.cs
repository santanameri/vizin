using vizin.DTO.Booking;
using vizin.Models;
using vizin.Models.Enum;

namespace vizin.Services.Booking;

public class BookingMapper
{
    public static BookingResponseDto ToDto(TbBooking booking, TbProperty property)
    {
        var totalNights = (booking.CheckoutDate.Date - booking.CheckinDate.Date).Days;
        if (totalNights <= 0) totalNights = 1;
        
        return new BookingResponseDto()
        {
            Id = booking.Id,
            PropertyTitle = property.Title,
            CheckIn = booking.CheckinDate,
            CheckOut = booking.CheckoutDate,
            TotalNights = totalNights,
            Status = (StatusBookingType)booking.Status,
            TotalCost = booking.TotalCost,
        };
    }
}