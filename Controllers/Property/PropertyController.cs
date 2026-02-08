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

    [HttpGet(Name = "GetAllProperty")]
    public List<PropertyResponseDto> GetAllProperty()
    {
        return _service.GetProperties();
    }
}