using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vizin.DTO.Property;
using vizin.DTO.Property.Amenity;
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

    [Authorize(Policy = "HospedeOnly")]
    [HttpGet]
    public async Task<IActionResult> GetAllProperty()
    {
        var properties = await _service.GetProperties();
        return Ok(properties);
    }
    
    [Authorize(Policy = "AnfitriaoOnly")]
    [HttpGet("my")]
    public async Task<IActionResult> GetAllPropertyByHost()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var userId = Guid.Parse(userIdClaim);
        var properties = await _service.GetPropertiesByHost(userId);
        return Ok(properties);
    }

    [Authorize(Policy = "AnfitriaoOnly")]
    [HttpPut("{propertyId:guid}")]
    public async Task<IActionResult> UpdateProperty([FromBody] PropertyCreateDto dto, [FromRoute] Guid propertyId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
       
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
    
    [Authorize(Policy = "AnfitriaoOnly")]
    [HttpPatch("{propertyId:guid}")]
    public async Task<IActionResult> UpdateDailyValue(Guid propertyId, [FromBody] PropertyUpdateDailyValueDto dto)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = Guid.Parse(userIdClaim);

            var updated = await _service.UpdateDailyValueAsync(
                propertyId,
                userId,
                dto
            );

            return Ok(new
            {
                message = "Valor da diária atualizado com sucesso.",
                data = updated
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [Authorize(Policy= "AnfitriaoOnly")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PropertyCreateDto dto)
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

    [Authorize(Policy = "AnfitriaoOnly")]
    [HttpPost("add-amenity/{propertyId:guid}")]
    public async Task<IActionResult> CreateAmenity([FromRoute] Guid propertyId, [FromBody] AddAmenityDto dto)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        try
        {
            var result = await _service.AddAmenitiesAsync(dto.AmenityId, propertyId, userId);
            return Ok(new { result, message = "Comodidade adicionada com sucesso!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Policy = "AnfitriaoOnly")]
    [HttpGet("amenities")]
    public async Task<IActionResult> GetAllAmenities()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();
        try
        {
            var result = await _service.GetAllAmenities();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}