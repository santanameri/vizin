using Moq;
using vizin.DTO.Booking;
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
        
        var dto = new PropertyCreateDto()
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
            await _service.UpdateProperty(new PropertyCreateDto(), userId, propertyId));
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

        Assert.ThrowsAsync<ArgumentException>(async () =>
            await _service.UpdateProperty(new PropertyCreateDto(), userId, propertyId));
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
            await _service.UpdateProperty(new PropertyCreateDto(), _userId, propertyId));
    }

    [Test]
    public void UpdateProperty_ShowError_WhenDailyValueInvalid()
    {
        var propertyId = Guid.NewGuid();

        _propertyRepoMock
            .Setup(x => x.GetPropertyById(propertyId))
            .ReturnsAsync(new TbProperty { UserId = _userId });

        _userRepoMock
            .Setup(x => x.SelectUserById(_userId))
            .ReturnsAsync(new TbUser { Id = _userId });

        var dto = new PropertyCreateDto() { DailyValue = 0 };

        Assert.ThrowsAsync<ArgumentException>(async () =>
            await _service.UpdateProperty(dto, _userId, propertyId));
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
                         .ReturnsAsync((TbProperty p) => p); // Retorna o próprio objeto que recebeu

        // ACT
        var result = await _service.CreateProperty(dto, _userId);

        // ASSERT
        Assert.Multiple(() => {
            Assert.That(result.Title, Is.EqualTo(dto.Title));
            Assert.That(result.DailyValue, Is.EqualTo(dto.DailyValue));
            _propertyRepoMock.Verify(r => r.Create(It.IsAny<TbProperty>()), Times.Once);
        });
    }
    
    //aqui
   [Test]
    public async Task UpdateDailyValueAsync_WhenValid_ShouldUpdateAndReturnDto()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var dto = new PropertyUpdateDailyValueDto { DailyValue = 250.00m };
        
        var property = new vizin.Models.TbProperty 
        { 
            Id = propertyId, 
            UserId = _userId, 
            DailyValue = 100.00m 
        };

        _propertyRepoMock.Setup(r => r.GetPropertyById(propertyId))
                         .ReturnsAsync(property);

        // Act
        var result = await _service.UpdateDailyValueAsync(propertyId, _userId, dto);

        // Assert
        Assert.That(result.DailyValue, Is.EqualTo(250.00m));
        _propertyRepoMock.Verify(r => r.PatchAsync(It.Is<vizin.Models.TbProperty>(p => p.DailyValue == 250.00m)), Times.Once);
    }

    [Test]
    public void UpdateDailyValueAsync_WhenPropertyNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        _propertyRepoMock.Setup(r => r.GetPropertyById(It.IsAny<Guid>()))
                         .ReturnsAsync((vizin.Models.TbProperty)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () => 
            await _service.UpdateDailyValueAsync(Guid.NewGuid(), _userId, new PropertyUpdateDailyValueDto()));
        
        Assert.That(ex.Message, Is.EqualTo("Propriedade não encontrada"));
    }

    [Test]
    public void UpdateDailyValueAsync_WhenUserIsNotOwner_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var strangerId = Guid.NewGuid();
        
        var property = new vizin.Models.TbProperty { Id = propertyId, UserId = ownerId };

        _propertyRepoMock.Setup(r => r.GetPropertyById(propertyId))
                         .ReturnsAsync(property);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () => 
            await _service.UpdateDailyValueAsync(propertyId, strangerId, new PropertyUpdateDailyValueDto { DailyValue = 100 }));
            
        Assert.That(ex.Message, Is.EqualTo("Você não é o proprietário deste imóvel."));
    }

    [TestCase(0)]
    [TestCase(-50)]
    public void UpdateDailyValueAsync_WhenValueIsInvalid_ShouldThrowArgumentException(decimal invalidValue)
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var dto = new PropertyUpdateDailyValueDto { DailyValue = invalidValue };
        var property = new vizin.Models.TbProperty { Id = propertyId, UserId = _userId };

        _propertyRepoMock.Setup(r => r.GetPropertyById(propertyId))
                         .ReturnsAsync(property);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () => 
            await _service.UpdateDailyValueAsync(propertyId, _userId, dto));
        
        Assert.That(ex.Message, Is.EqualTo("O valor da diária deve ser maior que zero."));
    }

    [Test]
    public void UpdateDailyValueAsync_WhenValueIsNull_ShouldThrowArgumentException()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        
        // Criando o DTO com DailyValue nulo
        var dto = new PropertyUpdateDailyValueDto { DailyValue = null };
        
        var property = new vizin.Models.TbProperty 
        { 
            Id = propertyId, 
            UserId = _userId, 
            DailyValue = 100.00m 
        };

        _propertyRepoMock.Setup(r => r.GetPropertyById(propertyId))
                         .ReturnsAsync(property);

        // Act & Assert
        // Verifica se o código lança ArgumentException quando o valor é null
        var ex = Assert.ThrowsAsync<ArgumentException>(async () => 
            await _service.UpdateDailyValueAsync(propertyId, _userId, dto));
        
        Assert.That(ex.Message, Is.EqualTo("O valor da diária deve ser maior que zero."));
        
        // Garante que o repositório NUNCA foi chamado para salvar, já que falhou na validação
        _propertyRepoMock.Verify(r => r.PatchAsync(It.IsAny<vizin.Models.TbProperty>()), Times.Never);
    }

    //até aqui

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
        
        var result = await _service.AddAmenitiesAsync(amenityId, propertyId, _userId);
        
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

        var ex = Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.AddAmenitiesAsync(amenityId, propertyId, _userId));

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

        var ex = Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.AddAmenitiesAsync(amenityId, propertyId, _userId));

        Assert.That(ex!.Message, Is.EqualTo("Comodidade não encontrada"));
    }

    [Test]
    public void AddAmenitiesAsync_ShouldThrow_WhenAmenityReturnsNull()
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

        var ex = Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.AddAmenitiesAsync(amenityId, propertyId, _userId));

        Assert.That(ex!.Message, Is.EqualTo("Comodidade não encontrada"));
    }
    
    [Test]
    public void AddAmenitiesAsync_ShouldThrowException_WhenAmenityAlreadyExists()
    {
        var propertyId = Guid.NewGuid();
        var amenityId = Guid.NewGuid();
        
        var property = new TbProperty { Id = propertyId };
        _propertyRepoMock.Setup(r => r.GetPropertyById(propertyId))
            .ReturnsAsync(property);
        
        var amenity = new TbAmenity { Id = amenityId, Name = "WiFi" };
        _propertyRepoMock.Setup(r => r.GetAmenityById(amenityId))
            .ReturnsAsync(amenity);
        
        _propertyRepoMock.Setup(r => r.AddAmenityAsync(amenityId, propertyId))
            .ThrowsAsync(new InvalidOperationException("Essa comodidade já está cadastrada."));
        
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => 
            await _service.AddAmenitiesAsync(amenityId, propertyId, _userId));

        Assert.That(ex.Message, Is.EqualTo("Essa comodidade já está cadastrada."));
    }

    [Test]
    public async Task Search_WhenFilteringByCityCaseInsensitive_ShouldReturnCorrectProperties()
    {
        var filters = new PropertyFilterParams { Cidade = "teresina" };
        
        var fakeProperties = new List<TbProperty> 
        { 
            new TbProperty { Id = new Guid(), FullAddress = "Rua A, Teresina - PI" } 
        };
        
        _propertyRepoMock
            .Setup(repo => repo.SearchWithFiltersAsync(It.IsAny<PropertyFilterParams>()))
            .ReturnsAsync(fakeProperties);
        
        var result = await _service.FilterProperties(filters);
        
        Assert.That(result, Is.Not.Null);
        _propertyRepoMock.Verify(repo => repo.SearchWithFiltersAsync(It.Is<PropertyFilterParams>(f => f.Cidade == "teresina")), Times.Once);
    }
    
    [Test]
    public async Task RemoveAmenityAsync_ShouldReturnDto_WhenRemovalIsSuccessful()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var amenityId = Guid.NewGuid();
        var property = new TbProperty { Id = propertyId, UserId = _userId, Amenities = new List<TbAmenity>() };

        _propertyRepoMock.Setup(r => r.GetPropertyById(propertyId))
            .ReturnsAsync(property);

        _propertyRepoMock.Setup(r => r.RemoveAmenityAsync(amenityId, propertyId))
            .ReturnsAsync(property);

        // Act
        var result = await _service.RemoveAmenityAsync(amenityId, propertyId, _userId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(propertyId));
        _propertyRepoMock.Verify(r => r.RemoveAmenityAsync(amenityId, propertyId), Times.Once);
    }

    [Test]
    public void RemoveAmenityAsync_ShouldThrowKeyNotFoundException_WhenPropertyDoesNotExist()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        _propertyRepoMock.Setup(r => r.GetPropertyById(propertyId))
            .ReturnsAsync((TbProperty)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () => 
            await _service.RemoveAmenityAsync(Guid.NewGuid(), propertyId, _userId));
        
        Assert.That(ex.Message, Is.EqualTo("Imóvel não encontrado"));
    }

    [Test]
    public void RemoveAmenityAsync_ShouldThrowUnauthorizedAccessException_WhenUserIsNotTheOwner()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var property = new TbProperty { Id = propertyId, UserId = differentUserId };

        _propertyRepoMock.Setup(r => r.GetPropertyById(propertyId))
            .ReturnsAsync(property);

        // Act & Assert
        Assert.ThrowsAsync<UnauthorizedAccessException>(async () => 
            await _service.RemoveAmenityAsync(Guid.NewGuid(), propertyId, _userId));
    }

    [Test]
    public void RemoveAmenityAsync_ShouldThrowKeyNotFoundException_WhenRepositoryReturnNullOnUpdate()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var amenityId = Guid.NewGuid();
        var property = new TbProperty { Id = propertyId, UserId = _userId };

        _propertyRepoMock.Setup(r => r.GetPropertyById(propertyId))
            .ReturnsAsync(property);

        _propertyRepoMock.Setup(r => r.RemoveAmenityAsync(amenityId, propertyId))
            .ReturnsAsync((TbProperty)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () => 
            await _service.RemoveAmenityAsync(amenityId, propertyId, _userId));
            
        Assert.That(ex.Message, Is.EqualTo("Erro ao processar a remoção."));
    }

    //aqui
    [Test]
    public async Task FilterByAmenitiesAsync_ShouldThrowArgumentException_WhenAmenityIdsListIsEmpty()
    {
        // Arrange
        var emptyList = new List<Guid>();

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () => 
            await _service.FilterByAmenitiesAsync(emptyList, false));
        
        // Validando a mensagem exata definida na sua service
        Assert.That(ex.Message, Is.EqualTo("Nenhuma comodidade foi identificada."));
    }

    [Test]
    public async Task FilterByAmenitiesAsync_ShouldReturnMappedDtos_WhenRepositoryReturnsProperties()
    {
        // Arrange
        var amenityIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var matchAll = true;

        var fakeProperties = new List<TbProperty>
        {
            new TbProperty { Id = Guid.NewGuid(), Title = "Propriedade 1" },
            new TbProperty { Id = Guid.NewGuid(), Title = "Propriedade 2" }
        };

        _propertyRepoMock.Setup(r => r.GetPropertiesByAmenitiesAsync(amenityIds, matchAll))
                         .ReturnsAsync(fakeProperties);

        // Act
        var result = await _service.FilterByAmenitiesAsync(amenityIds, matchAll);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0].Title, Is.EqualTo("Propriedade 1"));
        
        // Verifica se o repositório foi chamado com os parâmetros exatos
        _propertyRepoMock.Verify(r => r.GetPropertiesByAmenitiesAsync(amenityIds, matchAll), Times.Once);
    }

    [Test]
    public async Task FilterByAmenitiesAsync_ShouldReturnEmptyList_WhenNoPropertiesMatch()
    {
        // Arrange
        var amenityIds = new List<Guid> { Guid.NewGuid() };
        _propertyRepoMock.Setup(r => r.GetPropertiesByAmenitiesAsync(It.IsAny<List<Guid>>(), It.IsAny<bool>()))
                         .ReturnsAsync(new List<TbProperty>());

        // Act
        var result = await _service.FilterByAmenitiesAsync(amenityIds, false);

        // Assert
        Assert.That(result, Is.Empty);
        _propertyRepoMock.Verify(r => r.GetPropertiesByAmenitiesAsync(It.IsAny<List<Guid>>(), It.IsAny<bool>()), Times.Once);
    }
    
    [Test]
    public async Task GetAvailableProperties_Success_ShouldReturnMappedDtos()
    {
        // Arrange
        var filter = new AvailabilityFilterDto 
        { 
            CheckIn = DateTime.UtcNow.AddDays(1), 
            CheckOut = DateTime.UtcNow.AddDays(5) 
        };

        var fakeProperties = new List<TbProperty> 
        { 
            new TbProperty { Id = Guid.NewGuid(), Title = "Casa Ativa", Availability = true } 
        };

        _propertyRepoMock.Setup(r => r.GetAvailablePropertiesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(fakeProperties);

        // Act
        var result = await _service.GetAvailableProperties(filter);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Title, Is.EqualTo("Casa Ativa"));
    }

    [Test]
    public void GetAvailableProperties_CheckInInPast_ShouldThrowException()
    {
        // Arrange
        var filter = new AvailabilityFilterDto { CheckIn = DateTime.UtcNow.AddDays(-1) };

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => 
            await _service.GetAvailableProperties(filter));
        
        Assert.That(ex.Message, Is.EqualTo("Check-out deve ser após o check-in"));
    }

    [Test]
    public void GetAvailableProperties_InvalidDateRange_ShouldThrowException()
    {
        // Arrange: Check-out antes do Check-in
        var filter = new AvailabilityFilterDto 
        { 
            CheckIn = DateTime.UtcNow.AddDays(5), 
            CheckOut = DateTime.UtcNow.AddDays(2) 
        };

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => 
            await _service.GetAvailableProperties(filter));
        
        Assert.That(ex.Message, Is.EqualTo("Check-out deve ser após o check-in"));
    }
    
    [Test]
    public async Task GetAvailableProperties_WhenPropertyIsReserved_ShouldReturnEmptyList()
    {
        // Arrange: Datas que o usuário quer buscar
        var filter = new AvailabilityFilterDto 
        { 
            CheckIn = DateTime.UtcNow.AddDays(10), 
            CheckOut = DateTime.UtcNow.AddDays(15) 
        };

        // Simulamos que o Repositório (que executa aquele SQL com LEFT JOIN/NOT EXISTS) 
        // não encontrou nenhum imóvel disponível para esse período específico.
        _propertyRepoMock.Setup(r => r.GetAvailablePropertiesAsync(filter.CheckIn, filter.CheckOut))
            .ReturnsAsync(new List<TbProperty>()); // Retorna lista vazia

        // Act
        var result = await _service.GetAvailableProperties(filter);

        // Assert
        Assert.That(result, Is.Empty, "A lista deveria estar vazia pois o imóvel está reservado no período.");
    
        // Verifica se o repositório foi chamado exatamente com as datas do filtro
        _propertyRepoMock.Verify(r => r.GetAvailablePropertiesAsync(filter.CheckIn, filter.CheckOut), Times.Once);
    }
}