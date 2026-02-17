using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using vizin.DTO.Favorite;
using vizin.Services.Favorite.Interfaces;

[ApiController]
[Route("[controller]")]
public class FavoriteController : ControllerBase
{
    private readonly IFavoriteService _service;

    public FavoriteController(IFavoriteService service) => _service = service;

    [Authorize(Policy = "HospedeOnly")]
    [HttpPost("toggle")]
    public async Task<IActionResult> ToggleFavorite([FromBody] FavoriteRequestDto dto)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        try 
        {
            // O método agora retorna um booleano
            bool isAdded = await _service.ToggleFavoriteAsync(userId, dto.PropertyId);
            
            string message = isAdded 
                ? "Propriedade adicionada aos favoritos" 
                : "Propriedade removida dos favoritos";

            return Ok(new { message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}