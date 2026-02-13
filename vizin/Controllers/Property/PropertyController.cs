using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vizin.DTO.Property;
using vizin.Services.Property.Interfaces;

namespace vizin.Controllers.Property;
[Authorize]
[ApiController]
[Route("[controller]")]
public class PropertyController : ControllerBase
{
    private readonly IPropertyService _service;

    public PropertyController(IPropertyService service)
    {
        _service = service;
    }

    [Authorize(Roles = "Hospede")]
    [HttpGet]
    public async Task<IActionResult> GetAllProperty()
    {
        var properties = await _service.GetProperties();
        return Ok(properties);
    }
    
    [Authorize(Roles = "Anfitriao")]
    [HttpGet("my")]
    public async Task<IActionResult> GetAllPropertyByHost()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var userId = Guid.Parse(userIdClaim);
        var properties = await _service.GetPropertiesByHost(userId);
        return Ok(properties);
    }

    [Authorize(Roles = "Anfitriao")]
    [HttpPut("{propertyId:guid}")]
    public async Task<IActionResult> UpdateProperty([FromBody] PropertyResponseDto dto, [FromRoute] Guid propertyId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
       
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized();
        }
        
        try
        {
            var userId = Guid.Parse(userIdClaim);
            var result = await _service.UpdateProperty(dto, userId, propertyId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [Authorize(Roles = "Anfitriao")]
    [HttpPost]
    public async Task<IActionResult> Create(
    [FromBody] PropertyCreateDto dto
    )
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        try
        {
            var userId = Guid.Parse(userIdClaim);
            PropertyResponseDto result =
                await _service.CreateProperty(dto, userId);

            return Ok(new {result, message = "O imóvel foi cadastrado com sucesso."});
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}