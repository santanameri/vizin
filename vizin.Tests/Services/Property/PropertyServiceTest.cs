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
    private Guid _userId;

    [SetUp]
    public void Setup()
    {
        _propertyRepoMock = new Mock<IPropertyRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _service = new PropertyService(_propertyRepoMock.Object, _userRepoMock.Object);
    }
    
    [Test]
    public async Task UpdateProperty_ShouldUpdate_WhenDataValid()
    {
        var propertyId = Guid.NewGuid();

        var property = new TbProperty
        {
            Id = propertyId,
            UserId = _userId,
            Title = "Antigo"
        };
        var user = new TbUser();
        _userId = user.Id;
        
        var dto = new PropertyResponseDto
        {
            Title = "Novo título",
            Description = "Desc",
            FullAddress = "Rua 1",
            Availability = true,
            DailyValue = 100,
            Capacity = 2,
            AccomodationType = 1,
            PropertyCategory = 2
        };

        _propertyRepoMock
            .Setup(x => x.GetPropertyById(propertyId))
            .ReturnsAsync(property);

        _userRepoMock
            .Setup(x => x.SelectUserById(_userId))
            .ReturnsAsync(user);

        _propertyRepoMock
            .Setup(x => x.Update(propertyId, property))
            .ReturnsAsync(property);

        var result = await _service.UpdateProperty(dto, _userId, propertyId);

        Assert.That(result.Title, Is.EqualTo("Novo título"));
    }
    
    [Test]
    public void UpdateProperty_ShowError_WhenPropertyDoesntExist()
    {
        var userId = _userId;
        var propertyId = Guid.NewGuid();

        _propertyRepoMock
            .Setup(x => x.GetPropertyById(propertyId))
            .ReturnsAsync((TbProperty)null);

        Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _service.UpdateProperty(new PropertyResponseDto(), userId, propertyId));
    }
    
    [Test]
    public void UpdateProperty_ShowError_WhenUserDoesntExist()
    {
        var userId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();

        _propertyRepoMock
            .Setup(x => x.GetPropertyById(propertyId))
            .ReturnsAsync(new TbProperty { UserId = userId });

        _userRepoMock
            .Setup(x => x.SelectUserById(userId))
            .ReturnsAsync((TbUser)null);

        Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _service.UpdateProperty(new PropertyResponseDto(), userId, propertyId));
    }
    
    [Test]
    public void UpdateProperty_ShowError_WhenUserIsNotTheOwner()
    {
        var propertyId = Guid.NewGuid();

        _propertyRepoMock
            .Setup(x => x.GetPropertyById(propertyId))
            .ReturnsAsync(new TbProperty { UserId = Guid.NewGuid() });

        _userRepoMock
            .Setup(x => x.SelectUserById(_userId))
            .ReturnsAsync(new TbUser { Id = _userId });

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.UpdateProperty(new PropertyResponseDto(), _userId, propertyId));
    }

    [Test]
    public void UpdateProperty_DeveErro_QuandoDailyValueInvalido()
    {
        var propertyId = Guid.NewGuid();

        _propertyRepoMock
            .Setup(x => x.GetPropertyById(propertyId))
            .ReturnsAsync(new TbProperty { UserId = _userId });

        _userRepoMock
            .Setup(x => x.SelectUserById(_userId))
            .ReturnsAsync(new TbUser { Id = _userId });

        var dto = new PropertyResponseDto { DailyValue = 0 };

        Assert.ThrowsAsync<ArgumentException>(async () =>
            await _service.UpdateProperty(dto, _userId, propertyId));
    }

    [Test]
    public void CreateProperty_ShouldThrowException_WhenUserIsNotHost()
    {
        var userId = Guid.NewGuid();
        // ARRANGE
        var dto = new PropertyCreateDto
        {
            Title = "Casa Luxo",
            Description = "Desc",
            FullAddress = "Rua 1",
            Availability = true,
            DailyValue = 100,
            Capacity = 2,
            AccomodationType = 1,
            PropertyCategory = 2
        };
        
        var user = new TbUser { Id = userId, Type = 2 }; 
        
        _userRepoMock.Setup(r => r.SelectUserById(userId))
            .ReturnsAsync(user);
        
        _propertyRepoMock
            .Setup(r => r.Create(It.IsAny<TbProperty>()))
            .Returns((TbProperty p) => p);

        // ACT & ASSERT
        var ex = Assert.ThrowsAsync<Exception>(async () => 
            await _service.CreateProperty(dto, userId));

        Assert.That(ex.Message, Is.EqualTo("Apenas usuários do tipo Anfitrião podem cadastrar imóveis"));
    }

    [Test]
    public async Task CreateProperty_ShouldReturnResponse_WhenEverythingIsValid()
    {
        // ARRANGE
        var dto = new PropertyCreateDto
        {
            Title = "Casa de Praia", 
            Description = "Desc",
            FullAddress = "Rua 1",
            Availability = true,
            DailyValue = 150.0m,
            Capacity = 2,
            AccomodationType = 1,
            PropertyCategory = 2
        };
        var user = new TbUser { Id = _userId, Type = 1 }; // Anfitrião válido

        _userRepoMock.Setup(r => r.SelectUserById(_userId)).ReturnsAsync(user);
        
        // Mockando o retorno do repositório de propriedade
        _propertyRepoMock.Setup(r => r.Create(It.IsAny<TbProperty>()))
                         .Returns((TbProperty p) => p); // Retorna o próprio objeto que recebeu

        // ACT
        var result = await _service.CreateProperty(dto, _userId);

        // ASSERT
        Assert.Multiple(() => {
            Assert.That(result.Title, Is.EqualTo(dto.Title));
            Assert.That(result.DailyValue, Is.EqualTo(dto.DailyValue));
            _propertyRepoMock.Verify(r => r.Create(It.IsAny<TbProperty>()), Times.Once);
        });
    }
    
    [Test]
    public async Task AddAmenitiesAsync_ShouldAddAmenity_WhenValid()
    {
        var propertyId = Guid.NewGuid();
        var amenityId = Guid.NewGuid();

        var amenity = new TbAmenity { Id = amenityId, Name = "WiFi" };

        var property = new TbProperty
        {
            Id = propertyId,
            Amenities = new List<TbAmenity>()
        };
        
        _propertyRepoMock
            .Setup(r => r.GetPropertyById(propertyId))
            .ReturnsAsync(property);

        _propertyRepoMock
            .Setup(r => r.GetAmenityById(amenityId))
            .ReturnsAsync(amenity);

        _propertyRepoMock
            .Setup(r => r.AddAmenityAsync(amenityId, propertyId))
            .Callback(() => property.Amenities.Add(amenity)) // simula banco
            .ReturnsAsync(property);
        
        var result = await _service.AddAmenitiesAsync(amenityId, propertyId);
        
        Assert.That(result.Amenities.Count, Is.EqualTo(1));
        _propertyRepoMock.Verify(
            r => r.AddAmenityAsync(amenityId, propertyId),
            Times.Once);
    }
    
    [Test]
    public void AddAmenitiesAsync_ShouldThrow_WhenPropertyNotFound()
    {
        var propertyId = Guid.NewGuid();
        var amenityId = Guid.NewGuid();

        _propertyRepoMock
            .Setup(r => r.GetPropertyById(propertyId))
            .ReturnsAsync((TbProperty?)null);

        var ex = Assert.ThrowsAsync<Exception>(() =>
            _service.AddAmenitiesAsync(amenityId, propertyId));

        Assert.That(ex!.Message, Is.EqualTo("Imóvel não encontrado"));
    }

    [Test]
    public void AddAmenitiesAsync_ShouldThrow_WhenAmenityNotFound()
    {
        var propertyId = Guid.NewGuid();
        var amenityId = Guid.NewGuid();

        var property = new TbProperty
        {
            Id = propertyId,
            Amenities = new List<TbAmenity>()
        };

        _propertyRepoMock
            .Setup(r => r.GetPropertyById(propertyId))
            .ReturnsAsync(property);

        _propertyRepoMock
            .Setup(r => r.GetAmenityById(amenityId))
            .ReturnsAsync((TbAmenity?)null);

        var ex = Assert.ThrowsAsync<Exception>(() =>
            _service.AddAmenitiesAsync(amenityId, propertyId));

        Assert.That(ex!.Message, Is.EqualTo("Comodidade não encontrada"));
    }

    [Test]
    public void AddAmenitiesAsync_ShouldThrow_WhenAmenityAlreadyExists()
    {
        var propertyId = Guid.NewGuid();
        var amenityId = Guid.NewGuid();

        var amenity = new TbAmenity { Id = amenityId, Name = "WiFi" };

        var property = new TbProperty
        {
            Id = propertyId,
            Amenities = new List<TbAmenity> { amenity }
        };

        _propertyRepoMock
            .Setup(r => r.GetPropertyById(propertyId))
            .ReturnsAsync(property);

        _propertyRepoMock
            .Setup(r => r.GetAmenityById(amenityId))
            .ReturnsAsync(amenity);

        var ex = Assert.ThrowsAsync<Exception>(() =>
            _service.AddAmenitiesAsync(amenityId, propertyId));

        Assert.That(ex!.Message, Is.EqualTo("Essa comodidade já está cadastrada."));
    }

}