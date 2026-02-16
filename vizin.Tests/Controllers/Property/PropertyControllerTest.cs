using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Moq;
using Microsoft.AspNetCore.Mvc;
using vizin.Controllers.Property;
using vizin.Services.Property.Interfaces;
using vizin.DTO.Property;
using vizin.DTO.Property.Amenity;

namespace vizin.Tests.Controllers;

[TestFixture]
public class PropertyControllerTests
{
    private Mock<IPropertyService> _serviceMock;
    private PropertyController _controller;
    private Guid _userId;

    [SetUp]
    public void Setup()
    {
        _serviceMock = new Mock<IPropertyService>();
        _controller = new PropertyController(_serviceMock.Object);
        _userId = Guid.NewGuid();
        
        var user = new ClaimsPrincipal(new  ClaimsIdentity(new Claim[]
        {
            new  Claim(ClaimTypes.NameIdentifier, _userId.ToString()),
        }, "mock"));
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
    }
    
    [Test]
    public async Task UpdateProperty_ShouldReturnOk_WhenSuccessful()
    {
        // ARRANGE
        var propertyId = Guid.NewGuid();
        var dto = new PropertyCreateDto()
        {
            Title = "Novo título",
            DailyValue = 200
        };

        var expectedResponse = new PropertyResponseDto()
        {
            Title = "Novo título",
            DailyValue = 200
        };

        _serviceMock
            .Setup(s => s.UpdateProperty(dto, _userId, propertyId))
            .ReturnsAsync(expectedResponse);

        // ACT
        var result = await _controller.UpdateProperty(dto, propertyId);

        // ASSERT
        Assert.That(result, Is.TypeOf<OkObjectResult>());

        var ok = result as OkObjectResult;
        Assert.That(ok!.Value, Is.EqualTo(expectedResponse));

        _serviceMock.Verify(
            s => s.UpdateProperty(dto, _userId, propertyId),
            Times.Once);
    }
    
    [Test]
    public async Task UpdateProperty_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // ARRANGE
        var propertyId = Guid.NewGuid();
        var dto = new PropertyCreateDto()
        {
            Title = "Teste",
            DailyValue = 0
        };

        const string errorMessage = "Erro ao atualizar";

        _serviceMock
            .Setup(s => s.UpdateProperty(dto, _userId, propertyId))
            .ThrowsAsync(new Exception(errorMessage));

        // ACT
        var result = await _controller.UpdateProperty(dto, propertyId);

        // ASSERT
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());

        var badRequest = result as BadRequestObjectResult;

        Assert.That(badRequest!.Value!.ToString(),
            Does.Contain(errorMessage));
    }
    
    [Test]
    public async Task UpdateProperty_ShouldReturnUnauthorized_WhenUserIdClaimIsMissing()
    {
        // ARRANGE
        var propertyId = Guid.NewGuid();
        var dto = new PropertyCreateDto();

        // Remove o usuário do contexto
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // ACT
        var result = await _controller.UpdateProperty(dto, propertyId);

        // ASSERT
        Assert.That(result, Is.TypeOf<UnauthorizedResult>());
    }

    [Test]
    public async Task GetAllProperty_ShouldReturnOk_WithPropertyList()
    {
        var mockList = new List<PropertyResponseDto> { new PropertyResponseDto() };
        _serviceMock.Setup(s => s.GetProperties()).ReturnsAsync(mockList);
        
        var result = await _controller.GetAllProperty();

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        
        var ok = result as OkObjectResult;
        Assert.That(ok.Value, Is.EqualTo(mockList));
        _serviceMock.Verify(s => s.GetProperties(), Times.Once);
    }

    [Test]
    public async Task Create_ShouldReturnOk_WhenSuccessful()
    {
        var dto = new PropertyCreateDto();
        var responseDto = new PropertyResponseDto();

        _serviceMock.Setup(s => s.CreateProperty(dto, _userId))
                    .ReturnsAsync(responseDto);
        
        var result = await _controller.Create(dto);
        
        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var ok = result as OkObjectResult;
        Assert.That(ok.Value!.ToString(), Does.Contain("O imóvel foi cadastrado com sucesso."));
    }

    [Test]
    public async Task Create_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        var dto = new PropertyCreateDto();
        const string errorMessage = "Erro ao criar propriedade";

        _serviceMock.Setup(s => s.CreateProperty(dto, _userId))
                    .ThrowsAsync(new Exception(errorMessage));
        
        var result = await _controller.Create(dto);
        
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        
        var badRequest = result as BadRequestObjectResult;
        Assert.That(badRequest!.Value!.ToString(), Does.Contain(errorMessage));
    }

    [Test]
    public async Task UpdateDailyValue_ShouldReturnOk_WhenUpdateIsSuccessful()
    {
        // Arrange
        var propertyId = Guid.NewGuid();

        var dto = new PropertyUpdateDailyValueDto
        {
            DailyValue = 150
        };

        var serviceResponse = new PropertyResponseDto
        {
            DailyValue = 150
        };

        _serviceMock
            .Setup(s => s.UpdateDailyValueAsync(
                propertyId,
                _userId,
                It.Is<PropertyUpdateDailyValueDto>(d => d.DailyValue == 150)))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.UpdateDailyValue(propertyId, dto);

        // Assert
        var okResult = result as OkObjectResult;

        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
    }

    [Test]
    public async Task UpdateDailyValue_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        var propertyId = Guid.NewGuid();

        var dto = new PropertyUpdateDailyValueDto
        {
            DailyValue = 150
        };

        _controller.ModelState.AddModelError("DailyValue", "Required");

        // Act
        var result = await _controller.UpdateDailyValue(propertyId, dto);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());

        _serviceMock.Verify(
            s => s.UpdateDailyValueAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<PropertyUpdateDailyValueDto>()),
            Times.Never);
    }

    [Test]
    public async Task UpdateDailyValue_ShouldReturnNotFound_WhenKeyNotFoundExceptionIsThrown()
    {
        // Arrange
        var propertyId = Guid.NewGuid();

        var dto = new PropertyUpdateDailyValueDto
        {
            DailyValue = 200
        };

        _serviceMock
            .Setup(s => s.UpdateDailyValueAsync(
                propertyId,
                _userId,
                It.IsAny<PropertyUpdateDailyValueDto>()))
            .ThrowsAsync(new KeyNotFoundException("Property not found"));

        // Act
        var result = await _controller.UpdateDailyValue(propertyId, dto);

        // Assert
        var notFoundResult = result as NotFoundObjectResult;

        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult!.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
    }

    [Test]
    public async Task UpdateDailyValue_ShouldReturnBadRequest_WhenArgumentExceptionIsThrown()
    {
        // Arrange
        var propertyId = Guid.NewGuid();

        var dto = new PropertyUpdateDailyValueDto
        {
            DailyValue = -10
        };

        _serviceMock
            .Setup(s => s.UpdateDailyValueAsync(
                propertyId,
                _userId,
                It.IsAny<PropertyUpdateDailyValueDto>()))
            .ThrowsAsync(new ArgumentException("Invalid value"));

        // Act
        var result = await _controller.UpdateDailyValue(propertyId, dto);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;

        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult!.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
    }

    
    [Test]
    public async Task CreateAmenity_ShouldReturnOk_WhenValid()
    {
        var propertyId = Guid.NewGuid();
        var amenityId = Guid.NewGuid();
        var userId = _userId;
        
        var dto = new AddAmenityDto { AmenityId = amenityId };

        _serviceMock
            .Setup(s => s.AddAmenitiesAsync(amenityId, propertyId, _userId))
            .ReturnsAsync(new PropertyResponseDto());

        var result = await _controller.CreateAmenity(propertyId, dto);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());

        _serviceMock.Verify(
            s => s.AddAmenitiesAsync(amenityId, propertyId, _userId),
            Times.Once);
    }
    
    [Test]
    public async Task AddAmenity_ShouldReturnUnauthorized_WhenUserMissing()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext() // sem user
        };

        var result = await _controller.CreateAmenity(Guid.NewGuid(), new AddAmenityDto());

        Assert.That(result, Is.InstanceOf<UnauthorizedResult>());
    }
    
    [Test]
    public async Task GetFiltered_WhenParametersAreValid_ShouldReturnOk()
    {
      
        var filters = new PropertyFilterParams { Cidade = "Teresina" };
        var expectedDtoList = new List<PropertyResponseDto> 
        { 
            new PropertyResponseDto { Id = new Guid(), FullAddress = "Endereço Teste" } 
        };
      
        _serviceMock
            .Setup(s => s.FilterProperties(It.IsAny<PropertyFilterParams>()))
            .ReturnsAsync(expectedDtoList);
     
        var result = await _controller.GetAllPropertyFilters(filters);
        
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.StatusCode, Is.EqualTo(200));
        Assert.That(okResult.Value, Is.EqualTo(expectedDtoList));
    }

    [Test]
    public async Task GetFiltered_WhenNoPropertiesFound_ShouldReturnNotFound()
    {
        _serviceMock
            .Setup(s => s.FilterProperties(It.IsAny<PropertyFilterParams>()))
            .ReturnsAsync(new List<PropertyResponseDto>()); // Lista vazia
        
        var result = await _controller.GetAllPropertyFilters(new PropertyFilterParams());

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task RemoveAmenity_ShouldReturnOk_WhenRemovalIsSuccessful()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var amenityId = Guid.NewGuid();
        var expectedResult = new PropertyResponseDto { Id = propertyId };

        _serviceMock.Setup(s => s.RemoveAmenityAsync(amenityId, propertyId, _userId))
            .ReturnsAsync(expectedResult);

        // Act
        var actionResult = await _controller.RemoveAmenity(propertyId, amenityId);

        // Assert
        var result = actionResult as OkObjectResult;
        
        // Nova sintaxe NUnit recomendada:
        Assert.That(result, Is.Not.Null); 
        Assert.That(result.StatusCode, Is.EqualTo(200));
        
        // Para acessar o objeto anônimo com segurança
        var message = result.Value.GetType().GetProperty("message").GetValue(result.Value, null) as string;
        Assert.That(message, Is.EqualTo("Comodidade removida com sucesso!"));
    }

    [Test]
    public async Task RemoveAmenity_ShouldReturnNotFound_WhenKeyNotFoundExceptionIsThrown()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var amenityId = Guid.NewGuid();
        var errorMessage = "Imóvel não encontrado";

        _serviceMock.Setup(s => s.RemoveAmenityAsync(amenityId, propertyId, _userId))
            .ThrowsAsync(new KeyNotFoundException(errorMessage));

        // Act
        var actionResult = await _controller.RemoveAmenity(propertyId, amenityId);

        // Assert
        var result = actionResult as NotFoundObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(404));
        
        var message = result.Value.GetType().GetProperty("message").GetValue(result.Value, null) as string;
        Assert.That(message, Is.EqualTo(errorMessage));
    }


}
