using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using vizin.Controllers.Booking;
using vizin.DTO.Booking;
using vizin.Models.Enum;
using vizin.Services.Booking.Interfaces;

namespace vizin.Tests.Controllers;

[TestFixture]
public class BookingControllerTests
{
    private Mock<IBookingService> _serviceMock;
    private BookingController _controller;
    private Guid _userId;

    [SetUp]
    public void SetUp()
    {
        _serviceMock = new Mock<IBookingService>();
        _controller = new BookingController(_serviceMock.Object);
        _userId = Guid.NewGuid();
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        // Injetando o usuário no contexto do Controller
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Test]
    public async Task CreateBooking_Success_ShouldReturnOk()
    {
        var propertyId = Guid.NewGuid();
        var dto = new CreateBookingDto { CheckIn = DateTime.Today.AddDays(1), CheckOut = DateTime.Today.AddDays(2), GuestCount = 2 };
        
        var expectedResponse = new BookingResponseDto 
        { 
            Id = Guid.NewGuid(), 
            PropertyTitle = "Imóvel de Teste",
            Status = StatusBookingType.Criado 
        };

        _serviceMock.Setup(s => s.CreateBooking(_userId, propertyId, dto))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.CreateBooking(propertyId, dto);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult.Value, Is.EqualTo(expectedResponse));
    }

    [Test]
    public async Task CreateBooking_ServiceThrowsException_ShouldReturnBadRequest()
    {
  
        var propertyId = Guid.NewGuid();
        var dto = new CreateBookingDto();
        var errorMessage = "Imóvel já reservado para este período.";

        _serviceMock.Setup(s => s.CreateBooking(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CreateBookingDto>()))
            .ThrowsAsync(new Exception(errorMessage));

        var result = await _controller.CreateBooking(propertyId, dto);
        
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequest = result as BadRequestObjectResult;
        
        dynamic value = badRequest.Value;
        Assert.That(value.GetType().GetProperty("message").GetValue(value, null), Is.EqualTo(errorMessage));
    }
}