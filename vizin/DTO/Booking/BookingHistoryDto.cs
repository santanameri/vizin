using vizin.DTO.Booking; 

namespace vizin.DTO.Booking; 
public class BookingHistoryDto
{
    public List<BookingResponseDto> Ongoing { get; set; } = new();
    public List<BookingResponseDto> Past { get; set; } = new();
}