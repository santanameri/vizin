using Moq;
using vizin.DTO.User;
using vizin.Models;
using vizin.Models.Enum;
using vizin.Repositories.User;
using vizin.Services.User;

namespace vizin.Tests.Services.User;

[TestFixture]
public class UserServiceTest
{
    private Mock<IUserRepository> _repositoryMock;
    private UserService _service;

    [SetUp]
    public void SetUp()
    {
        _repositoryMock = new Mock<IUserRepository>();
        _service = new UserService(_repositoryMock.Object);
    }
    
    [Test]
    public async Task LoginUser_ShouldReturnUser_WhenPasswordCorrect()
    {
        var email = "teste@vizin.com";
        var senhaPura = "senha123";
        var senhaHash = BCrypt.Net.BCrypt.HashPassword(senhaPura);
        var usuarioFake = new TbUser { Email = email, Password = senhaHash };

        _repositoryMock.Setup(r => r.HandleLogin(email)).ReturnsAsync(usuarioFake);
        
        var result = await _service.LoginUser(email, senhaPura);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Email, Is.EqualTo(email));
    }

    [Test]
    public void LoginUser_ShouldThrowError_WhenPasswordIncorrect()
    {
        var email = "teste@vizin.com";
        var senhaHash = BCrypt.Net.BCrypt.HashPassword("senha_correta");
        var usuarioFake = new TbUser { Email = email, Password = senhaHash };

        _repositoryMock.Setup(r => r.HandleLogin(email)).ReturnsAsync(usuarioFake);
        
        var ex = Assert.ThrowsAsync<Exception>(async () => await _service.LoginUser(email, "senha_errada"));
        Assert.That(ex.Message, Is.EqualTo("A senha está incorreta!"));
    }
    
    [Test]
    public async Task CreateUser_ShouldCreateUser_WhenDataValid()
    {
        var request = new CreateUserRequestDTO { Name = "Luiz", Email = "novo@email.com", Password = "123", Type = (UserType)1 };
    
        _repositoryMock.Setup(r => r.GetUserByEmailAsync(request.Email)).ReturnsAsync((TbUser)null);
        _repositoryMock.Setup(r => r.CreateUserAsync(It.IsAny<TbUser>())).ReturnsAsync(new TbUser 
        { 
            Id = Guid.NewGuid(), 
            Name = request.Name, 
            Email = request.Email 
        });
        
        var result = await _service.CreateUser(request);
        
        Assert.That(result.Email, Is.EqualTo(request.Email));
        _repositoryMock.Verify(r => r.CreateUserAsync(It.IsAny<TbUser>()), Times.Once);
    }
    
    [Test]
    public void CreateUser_ShouldThrowError_WhenEmailAlreadyExists()
    {
        var request = new CreateUserRequestDTO { Email = "jaexiste@vizin.com" };
        _repositoryMock.Setup(r => r.GetUserByEmailAsync(request.Email)).ReturnsAsync(new TbUser());
        
        Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.CreateUser(request));
    }
}