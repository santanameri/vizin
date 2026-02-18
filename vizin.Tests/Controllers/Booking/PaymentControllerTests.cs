using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using vizin.Controllers.Payment;
using vizin.DTO.Booking;
using vizin.Models.Enum;
using vizin.Services.Booking.Interfaces;

namespace vizin.Tests.Controllers;

[TestFixture]
public class PaymentControllerTests
{
    private Mock<IPaymentService> _paymentServiceMock;
    private PaymentController _controller;
    private Guid _userId;

    [SetUp]
    public void SetUp()
    {
        _paymentServiceMock = new Mock<IPaymentService>();
        _controller = new PaymentController(_paymentServiceMock.Object);
        _userId = Guid.NewGuid();
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = claimsPrincipal }
        };
    }

    [Test]
    public async Task Pay_ValidRequest_ReturnsOk()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var dto = new PaymentRequestDto { CardNumber = "440019779871", Method = PaymentMethodType.Boleto };
        var expectedResponse = new PaymentResponseDto { Status = "Aprovado", Success = true };

        _paymentServiceMock.Setup(s => s.ProcessPaymentAsync(
                It.IsAny<Guid>(), 
                It.IsAny<Guid>(), 
                It.IsAny<PaymentRequestDto>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Pay(bookingId, dto);

        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult.Value, Is.EqualTo(expectedResponse));
    }

    [Test]
    public async Task Pay_ServiceThrowsException_ReturnsBadRequest()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var dto = new PaymentRequestDto();
        
        _paymentServiceMock.Setup(s => s.ProcessPaymentAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<PaymentRequestDto>()))
            .ThrowsAsync(new Exception("Reserva não encontrada."));

        // Act
        var result = await _controller.Pay(bookingId, dto);

        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        var badRequest = result as BadRequestObjectResult;
        // Verifica se a mensagem de erro da Service foi repassada para o cliente
        Assert.That(badRequest.Value.ToString(), Does.Contain("Reserva não encontrada."));
    }

    [Test]
    public async Task Pay_UserNotIdentified_ReturnsUnauthorized()
    {
        // Arrange: Removemos os Claims do contexto para simular usuário deslogado
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var result = await _controller.Pay(Guid.NewGuid(), new PaymentRequestDto());

        // Assert: Cobre a branch 'if (string.IsNullOrEmpty(userIdClaim))'
        Assert.That(result, Is.TypeOf<UnauthorizedObjectResult>());
    }

    [Test]
    public async Task Pay_PaymentRefused_ReturnsBadRequest()
    {
        // Arrange: Simula a Service retornando Success = false (sem lançar Exception)
        var responseDto = new PaymentResponseDto { Success = false, Status = "Cartão Recusado" };
        
        _paymentServiceMock.Setup(s => s.ProcessPaymentAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<PaymentRequestDto>()))
                        .ReturnsAsync(responseDto);

        // Act
        var result = await _controller.Pay(Guid.NewGuid(), new PaymentRequestDto());

        // Assert: Cobre a branch 'if (!response.Success)'
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        var badRequest = result as BadRequestObjectResult;
        Assert.That(badRequest.Value, Is.EqualTo(responseDto));
    }
}