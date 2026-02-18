namespace vizin.DTO.Booking;

public class BookingSummaryDto
{
    public string PropertyTitle { get; set; }
    public string GuestName { get; set; }
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public decimal TotalCost { get; set; }
    public string Status { get; set; }
}