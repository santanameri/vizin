using Moq;
using vizin.DTO.Property;
using vizin.Models;
using vizin.Repositories.Property.Interfaces;
using vizin.Repositories.User;
using vizin.Services.Property;

namespace vizin.Tests.Services.Property;

[TestFixture]
public class PropertyServiceTests
{
    private Mock<IPropertyRepository> _propertyRepoMock;
    private Mock<IUserRepository> _userRepoMock;
    private PropertyService _service;

    [SetUp]
    public void Setup()
    {
        _propertyRepoMock = new Mock<IPropertyRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _service = new PropertyService(_propertyRepoMock.Object, _userRepoMock.Object);
    }

    [Test]
    public async Task CreateProperty_ShouldThrowException_WhenUserIsNotHost()
    {
        // ARRANGE
        var userId = Guid.NewGuid();
        var dto = new PropertyCreateDto { Title = "Casa Luxo" };
        
        // Simulando um usuário que NÃO é anfitrião (Type != 1)
        var user = new TbUser { Id = userId, Type = 2 }; 

        _userRepoMock.Setup(r => r.SelectUserById(userId))
                     .ReturnsAsync(user);

        // ACT & ASSERT
        var ex = Assert.ThrowsAsync<Exception>(async () => 
            await _service.CreateProperty(dto, userId));

        Assert.That(ex.Message, Is.EqualTo("Apenas usuários do tipo Anfitrião podem cadastrar imóveis"));
    }

    [Test]
    public async Task CreateProperty_ShouldReturnResponse_WhenEverythingIsValid()
    {
        // ARRANGE
        var userId = Guid.NewGuid();
        var dto = new PropertyCreateDto { Title = "Casa de Praia", DailyValue = 150.00m };
        var user = new TbUser { Id = userId, Type = 1 }; // Anfitrião válido

        _userRepoMock.Setup(r => r.SelectUserById(userId)).ReturnsAsync(user);
        
        // Mockando o retorno do repositório de propriedade
        _propertyRepoMock.Setup(r => r.Create(It.IsAny<TbProperty>()))
                         .Returns((TbProperty p) => p); // Retorna o próprio objeto que recebeu

        // ACT
        var result = await _service.CreateProperty(dto, userId);

        // ASSERT
        Assert.Multiple(() => {
            Assert.That(result.Title, Is.EqualTo(dto.Title));
            Assert.That(result.DailyValue, Is.EqualTo(dto.DailyValue));
            _propertyRepoMock.Verify(r => r.Create(It.IsAny<TbProperty>()), Times.Once);
        });
    }
}