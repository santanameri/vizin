using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Moq;
using Microsoft.AspNetCore.Mvc;
using vizin.Controllers.Property;
using vizin.Services.Property.Interfaces;
using vizin.DTO.Property;

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
        var dto = new PropertyResponseDto
        {
            Title = "Novo título",
            DailyValue = 200
        };

        var expectedResponse = new PropertyResponseDto
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
        var dto = new PropertyResponseDto
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
        var dto = new PropertyResponseDto();

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
}