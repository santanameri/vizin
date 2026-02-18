using vizin.Models.Enum;

namespace vizin.DTO.Booking;
public class PaymentResponseDto
{
    public Guid PaymentId { get; set; }
    public string Status { get; set; } // "Pago", "Falhou", etc.
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Message { get; set; }
    public bool Success { get; set; }
}