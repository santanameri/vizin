using vizin.Models.Enum;

namespace vizin.DTO.Booking;

public class BookingResponseDto
{
    public Guid Id {get; set;}
    public string PropertyTitle {get; set;}
    public DateTime CheckIn {get; set;}
    public DateTime CheckOut {get; set;}
    public int TotalNights {get; set;}
    public decimal TotalCost {get; set;}
    public StatusBookingType Status {get; set;}
}