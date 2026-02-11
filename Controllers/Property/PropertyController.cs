using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vizin.DTO.Property;
using vizin.Services.Property.Interfaces;

namespace vizin.Controllers.Property;
//[Authorize]
[ApiController]
[Route("[controller]")]
public class PropertyController : ControllerBase
{
    private IPropertyService _service;

    public PropertyController(IPropertyService service)
    {
        _service = service;
    }

    [HttpGet]
    public List<PropertyResponseDto> GetAllProperty()
    {
    return _service.GetProperties();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(
    [FromBody] PropertyCreateDto dto
    // [FromHeader(Name = "user-id")] Guid userId
    )
    {
      
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { message = "Usuário não identificado no token" });
            }
            
            Guid userId = Guid.Parse(userIdClaim);
            PropertyResponseDto result =
                await _service.CreateProperty(dto, userId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}