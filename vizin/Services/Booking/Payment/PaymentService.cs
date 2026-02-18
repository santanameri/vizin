using vizin.DTO.Booking;
using vizin.Models;
using vizin.Models.Enum;
using vizin.Repositories.Booking.Interfaces;
using vizin.Services.Booking.Interfaces;

namespace vizin.Services.Booking.Payment;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepo;
    private readonly IBookingRepository _bookingRepo;

    public PaymentService(IPaymentRepository paymentRepo, IBookingRepository bookingRepo)
    {
        _paymentRepo = paymentRepo;
        _bookingRepo = bookingRepo;
    }

    public async Task<PaymentResponseDto> ProcessPaymentAsync(Guid bookingId, Guid userId, PaymentRequestDto dto)
    {
        var booking = await _bookingRepo.GetByIdAsync(bookingId);
        if (booking == null) throw new Exception("Reserva não encontrada.");
        if (booking.UserId != userId) throw new Exception("Usuário não autorizado para este pagamento.");
        
        if (booking.Status != (int)StatusBookingType.Criado)
            throw new Exception("Esta reserva não pode mais ser paga.");

        // Simulação de Gateway de Pagamento 
        var isPaymentApproved = SimulateGateway(dto);

        var payment = new TbPayment
        {
            Id = Guid.NewGuid(),
            BookingId = bookingId,
            Amount = booking.TotalCost,
            PaymentMethod= dto.Method,
            StatusPayment = (int)(isPaymentApproved ? StatusPaymentType.Aprovado : StatusPaymentType.Recusado),
            PaymentDate = DateTime.UtcNow
        };

        // Se aprovado, atualiza o status da reserva para Confirmado
        if (isPaymentApproved)
        {
            booking.Status = (int)StatusBookingType.Confirmado;
            await _bookingRepo.UpdateAsync(booking);
        }

        await _paymentRepo.CreateAsync(payment);

        return new PaymentResponseDto 
        { 
            PaymentId = payment.Id, 
            Status = ((StatusPaymentType)payment.StatusPayment).ToString(),
            Amount = payment.Amount 
        };
    }

    private bool SimulateGateway(PaymentRequestDto dto)
    {
        // Lógica fake: se o número do cartão começar com '4', aprova (simulação padrão)
        return !string.IsNullOrEmpty(dto.CardNumber) && dto.CardNumber.StartsWith("44");
    }
}