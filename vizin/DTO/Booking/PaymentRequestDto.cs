using vizin.Models.Enum;

namespace vizin.DTO.Booking;

public class PaymentRequestDto
{
    public string CardNumber { get; set; } // O número vem daqui!
    public string CardHolderName { get; set; }
    public string ExpirationDate { get; set; }
    public string CVV { get; set; }
    public PaymentMethodType Method { get; set; }
}