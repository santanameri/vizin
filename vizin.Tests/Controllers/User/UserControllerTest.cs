using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using vizin.Controllers;
using vizin.DTO.Login;
using vizin.DTO.User;
using vizin.Models;
using vizin.Models.Enum;
using vizin.Services.User.Interface;

namespace vizin.Tests.Controllers;

[TestFixture]
public class UserControllerTest
{
    private Mock<IUserService> _userServiceMock;
    private Mock<ITokenService> _tokenServiceMock;
    private UserController _controller;

    [SetUp]
    public void SetUp()
    {
        _userServiceMock = new Mock<IUserService>();
        _tokenServiceMock = new Mock<ITokenService>();
        _controller = new UserController(_userServiceMock.Object, _tokenServiceMock.Object);
    }
    
    [Test]
    public async Task Login_ShouldReturnOk_WhenTokenValid()
    {
        var loginDto = new LoginDto { Email = "user@vizin.com", Password = "123" };
        var userFake = new TbUser { Id = Guid.NewGuid(), Email = loginDto.Email, Type = 1 };
    
        _userServiceMock.Setup(s => s.LoginUser(loginDto.Email, loginDto.Password))
            .ReturnsAsync(userFake);
        _tokenServiceMock.Setup(s => s.GenerateToken(userFake))
            .Returns("token-de-teste");
        
        var result = await _controller.Login(loginDto);

        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task Login_ShouldReturnUnauthorized_WhenUserNull()
    {
        var loginDto = new LoginDto { Email = "errado@vizin.com", Password = "000" };
        _userServiceMock.Setup(s => s.LoginUser(loginDto.Email, loginDto.Password))
            .ReturnsAsync((TbUser)null);

        var result = await _controller.Login(loginDto);

        Assert.That(result, Is.TypeOf<UnauthorizedObjectResult>());
    }
    
    [Test]
    public async Task Create_ShouldReturnCreated_WhenSuccessful()
    {
        var request = new CreateUserRequestDTO { Name = "Luiz", Email = "luiz@vizin.com", Type = (UserType)1 };
        var response = new UserResponseDTO { Id = Guid.NewGuid(), Name = "Luiz", Type = 1 };

        _userServiceMock.Setup(s => s.CreateUser(request)).ReturnsAsync(response);
        
        var result = await _controller.Create(request);
        
        var createdResult = result as CreatedAtActionResult;
        Assert.That(createdResult, Is.Not.Null);
        Assert.That(createdResult.StatusCode, Is.EqualTo(201));
    }

    [Test]
    public async Task Create_ShouldReturnConflict_WhenEmailAlreadyExists()
    {
        var request = new CreateUserRequestDTO { Email = "repetido@vizin.com" };
        _userServiceMock.Setup(s => s.CreateUser(request))
            .ThrowsAsync(new InvalidOperationException("Email já está cadastrado!"));

        var result = await _controller.Create(request);

        var conflictResult = result as ConflictObjectResult;
        Assert.That(conflictResult, Is.Not.Null);
        var details = conflictResult.Value as ProblemDetails;
        Assert.That(details.Detail, Is.EqualTo("Email já está cadastrado!"));
    }
}