using vizin.DTO.ReviewRequestDto;
using vizin.Services.Review.Interfaces;

namespace vizin.Controllers.Review;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("[controller]")]
[Authorize] // Garante que apenas usuários logados acessem
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [Authorize("HospedeOnly")]
    [HttpPost("{bookingId}")]
    public async Task<IActionResult> Create(Guid bookingId, [FromBody] ReviewRequestDto dto)
    {
        try
        {
            // Pega o ID do usuário logado através do Token JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            var authorId = Guid.Parse(userIdClaim);

            var reviewId = await _reviewService.CreateBookingReviewAsync(authorId, bookingId, dto);

            return Ok(new { id = reviewId, message = "Avaliação enviada com sucesso!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [Authorize("HospedeOnly")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            var userId = Guid.Parse(userIdClaim);

            await _reviewService.DeleteReviewAsync(id, userId);

            return Ok(new { message = "Avaliação removida com sucesso." });
        }
        catch (Exception ex)
        {
            // Se cair aqui, pode ser porque a review não é dele ou não existe
            return BadRequest(new { message = ex.Message });
        }
    }
}