using Moq;
using NUnit.Framework;
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

    [SetUp]
    public void Setup()
    {
        // Inicializa o Mock da Interface
        _serviceMock = new Mock<IPropertyService>();
        
        // Instancia a Controller passando o Mock
        _controller = new PropertyController(_serviceMock.Object);
    }

    [Test]
    public void GetAllProperty_ShouldReturnPropertyList()
    {
        // Arrange 
        var mockList = new List<PropertyResponseDto> { new PropertyResponseDto() };
        _serviceMock.Setup(s => s.GetProperties()).Returns(mockList);

        // Act 
        var result = _controller.GetAllProperty();

        // Assert 
        Assert.That(result, Is.EqualTo(mockList));
        _serviceMock.Verify(s => s.GetProperties(), Times.Once);
    }

    [Test]
    public async Task Create_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var dto = new PropertyCreateDto();
        var userId = Guid.NewGuid();
        var responseDto = new PropertyResponseDto();

        _serviceMock.Setup(s => s.CreateProperty(dto, userId))
                    .ReturnsAsync(responseDto);

        // Act
        var result = await _controller.Create(dto, userId);

        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult.Value, Is.EqualTo(responseDto));
    }

    [Test]
    public async Task Create_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var dto = new PropertyCreateDto();
        var userId = Guid.NewGuid();
        var errorMessage = "Erro ao criar propriedade";

        _serviceMock.Setup(s => s.CreateProperty(dto, userId))
                    .ThrowsAsync(new Exception(errorMessage));

        // Act
        var result = await _controller.Create(dto, userId);

        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        var badRequest = result as BadRequestObjectResult;
        
        // Verifica se a mensagem de erro no JSON está correta
        Assert.That(badRequest.Value.ToString(), Does.Contain(errorMessage));
    }
}