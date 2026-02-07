using Microsoft.AspNetCore.Mvc;
using vizin.DTO.User;

namespace vizin.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _service;

    public UserController(IUserService service)
    {
        _service = service;
    }

    [HttpGet(Name = "GetAllUsers")]
    public List<UserResponseDTO> Get()
    {
        return _service.GetUsers();
    }
}
