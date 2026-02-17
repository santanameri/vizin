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

    [Test]
    public async Task GetMyBookings_ShouldReturnOk_WithHistory()
    {
        // Arrange: Configura uma role para o usuário no contexto do teste
        var role = "Hospede";
        var identity = (ClaimsIdentity)_controller.HttpContext.User.Identity!;
        identity.AddClaim(new Claim(ClaimTypes.Role, role));

        var expectedHistory = new BookingHistoryDto
        {
            Ongoing = new List<BookingResponseDto>(),
            Past = new List<BookingResponseDto>()
        };

        _serviceMock.Setup(s => s.GetUserBookingHistoryAsync(_userId, role))
            .ReturnsAsync(expectedHistory);

        // Act
        var result = await _controller.GetMyBookings();

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.StatusCode, Is.EqualTo(200));
        Assert.That(okResult.Value, Is.EqualTo(expectedHistory));
        
        // Verifica se o serviço foi chamado com o ID e a Role corretos extraídos do token
        _serviceMock.Verify(s => s.GetUserBookingHistoryAsync(_userId, role), Times.Once);
    }

    [Test]
    public async Task GetMyBookings_WhenUserIsAnfitriao_ShouldPassCorrectRoleToService()
    {
        // Arrange: Simula o cenário de Anfitrião
        var role = "Anfitriao";
        var identity = (ClaimsIdentity)_controller.HttpContext.User.Identity!;
        identity.AddClaim(new Claim(ClaimTypes.Role, role));

        var expectedHistory = new BookingHistoryDto();

        _serviceMock.Setup(s => s.GetUserBookingHistoryAsync(_userId, role))
            .ReturnsAsync(expectedHistory);

        // Act
        var result = await _controller.GetMyBookings();

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        _serviceMock.Verify(s => s.GetUserBookingHistoryAsync(_userId, role), Times.Once);
    }

    [Test]
    public async Task GetMyBookings_WhenRoleIsMissing_ShouldPassNullRoleToService()
    {
        // Arrange: O Setup padrão já tem o NameIdentifier, mas não tem a Role
        _serviceMock.Setup(s => s.GetUserBookingHistoryAsync(_userId, null))
            .ReturnsAsync(new BookingHistoryDto());

        // Act
        var result = await _controller.GetMyBookings();

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        
        // Verifica se passou null quando a claim de role não existe
        _serviceMock.Verify(s => s.GetUserBookingHistoryAsync(_userId, null), Times.Once);
    }

}