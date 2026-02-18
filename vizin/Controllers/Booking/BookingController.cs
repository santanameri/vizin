using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vizin.DTO.Booking;
using vizin.Services.Booking.Interfaces;

namespace vizin.Controllers.Booking;

[Authorize]
[ApiController]
[Route("[controller]")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _service;

    public BookingController(IBookingService service)
    {
        _service = service;
    }

    [Authorize(Policy = "HospedeOnly")]
    [HttpPost("{propertyId:guid}/book")]
    public async Task<IActionResult> CreateBooking([FromRoute] Guid propertyId, [FromBody] CreateBookingDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var userId = Guid.Parse(userIdClaim);
        try
        {
            var booking = await _service.CreateBooking(userId, propertyId, dto);
           return Ok(booking);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize] 
    [HttpGet("my-bookings")]
    public async Task<IActionResult> GetMyBookings()
    {
        // O código interno vai identificar quem é o usuário pelo Token
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        // Captura a Role (Papel) do usuário. 
        // Certifique-se de que o nome da Claim de Role seja o mesmo usado no seu login.
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value; 

        var history = await _service.GetUserBookingHistoryAsync(userId, userRole);
        return Ok(history);
    }
}