using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using vizin.Controllers.Review;
using vizin.DTO.ReviewRequestDto;
using vizin.Services.Review.Interfaces;

namespace vizin.Tests.Controllers.Review;

[TestFixture]
public class ReviewControllerTests
{
    private Mock<IReviewService> _serviceMock;
    private ReviewController _controller;

    [SetUp]
    public void SetUp()
    {
        _serviceMock = new Mock<IReviewService>();
        _controller = new ReviewController(_serviceMock.Object);
        
        // Simulação básica do usuário logado para o ControllerContext
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        }, "mock"));

        _controller.ControllerContext = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user } };
    }

    [Test]
    public async Task Create_ValidRequest_ReturnsOk()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var dto = new ReviewRequestDto { Stars = 5, Comment = "Ótimo!" };

        // Act
        var result = await _controller.Create(bookingId, dto);

        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task Delete_ServiceThrowsException_ReturnsBadRequest()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        _serviceMock.Setup(s => s.DeleteReviewAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ThrowsAsync(new Exception("Erro de permissão"));

        // Act
        var result = await _controller.Delete(reviewId);

        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Create_UserClaimMissing_ReturnsUnauthorized()
    {
        // Arrange: Removemos o usuário do contexto para simular ausência de Claim
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var result = await _controller.Create(Guid.NewGuid(), new ReviewRequestDto());

        // Assert
        Assert.That(result, Is.TypeOf<UnauthorizedResult>());
    }

    [Test]
    public async Task Create_ServiceThrowsException_ReturnsBadRequest()
    {
        // Arrange
        _serviceMock.Setup(s => s.CreateBookingReviewAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<ReviewRequestDto>()))
            .ThrowsAsync(new Exception("Erro genérico"));

        // Act
        var result = await _controller.Create(Guid.NewGuid(), new ReviewRequestDto());

        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Delete_UserClaimMissing_ReturnsUnauthorized()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var result = await _controller.Delete(Guid.NewGuid());

        // Assert
        Assert.That(result, Is.TypeOf<UnauthorizedResult>());
    }

    [Test]
    public async Task Delete_ValidRequest_ReturnsOk()
    {
        // Act
        var result = await _controller.Delete(Guid.NewGuid());

        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }
}