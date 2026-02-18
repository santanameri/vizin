using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using vizin.DTO.Booking;
using vizin.Services.Booking.Interfaces;

namespace vizin.Controllers.Payment;

[ApiController]
[Authorize(Policy = "HospedesOnly")] 
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("{bookingId}/pay")]
    public async Task<IActionResult> Pay(Guid bookingId, [FromBody] PaymentRequestDto dto)
    {
        try
        {
            // Pega o ID do usuário logado através do Claim do Token JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new { message = "Usuário não identificado." });

            var userId = Guid.Parse(userIdClaim);

            var response = await _paymentService.ProcessPaymentAsync(bookingId, userId, dto);

            if (!response.Success)
            {
                // Retornamos 200 ou 400 dependendo da regra de negócio. 
                // Para pagamentos recusados, costuma-se usar 400 (BadRequest)
                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}