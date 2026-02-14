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

    [HttpPost]
    public async Task<IActionResult> Create(
    [FromBody] PropertyCreateDto dto,
    [FromHeader(Name = "user-id")] Guid userId
    )
    {
        try
        {
            PropertyResponseDto result =
                await _service.CreateProperty(dto, userId);

            return Ok(new {result, message = "O imóvel foi cadastrado com sucesso."});
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{propertyId}/daily-value")]
    public async Task<IActionResult> UpdateDailyValue(
        Guid propertyId,
        [FromBody] PropertyUpdateDailyValueDto dto,
        [FromHeader(Name = "user-id")] Guid userId
    )
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var updated =
                await _service.UpdateDailyValueAsync(propertyId, userId, dto);
            
            Console.WriteLine("chamou o service");
            return Ok(new
            {
                message = "Valor da diária atualizada com sucesso.",
                data = updated
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

}