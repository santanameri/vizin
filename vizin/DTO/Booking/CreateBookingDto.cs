namespace vizin.DTO.Booking;

public class CreateBookingDto
{
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public int GuestCount { get; set; }
}