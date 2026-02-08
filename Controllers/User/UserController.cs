using Microsoft.AspNetCore.Mvc;
using vizin.DTO.Login;
using vizin.DTO.User;
using vizin.Models.Enum;
using vizin.Services.User.Interface;

namespace vizin.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _service;
    private readonly ITokenService _tokenService;

    public UserController(IUserService service, ITokenService tokenService)
    {
        _service = service;
        _tokenService = tokenService;
    }

    [HttpGet("{id:guid}", Name = "GetUser")]
    public Task<UserResponseDTO> Get(Guid id)
    {
        return _service.GetUser(id);
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

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto login)
    {
        var user = await _service.LoginUser(login.Email, login.Password);
        if (user == null)
        {
            return Unauthorized(new { message = "Email ou senha inválidos" });
        }

        var token = _tokenService.GenerateToken(user);
        return Ok( new
        {
            Token = token,
            UserId = user.Id,
            Role = ((UserType)user.Type).ToString()
        });
    }
}
