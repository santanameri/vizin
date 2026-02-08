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

    [HttpPost(Name = "CreatUser")]
    public async Task<IActionResult> Create([FromBody] CreateUserRequestDTO request)
    {
        try
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdUser = await _service.CreateUser(request);
            return CreatedAtAction(nameof(Get), new{id = createdUser.Id}, createdUser);
        } catch(InvalidOperationException ex)
        {
           var problemDetails = new ProblemDetails
           {
               Status = StatusCodes.Status409Conflict, Title = "Conflito", Detail = ex.Message
           };

           return Conflict(problemDetails);
        } catch(Exception ex)
        {
            var problemDetails = new ProblemDetails
            {
               Status = StatusCodes.Status500InternalServerError, Title = "Erro Interno do Servidor", Detail = ex.Message 
            };

            return StatusCode(StatusCodes.Status500InternalServerError, problemDetails);

        }
    }
    
}
