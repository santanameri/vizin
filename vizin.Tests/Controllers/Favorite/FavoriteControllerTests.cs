using Moq;
using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using vizin.Controllers;
using vizin.DTO.Favorite;
using vizin.Services.Favorite.Interfaces;
using System.Reflection;

namespace vizin.Tests.Controllers
{
    [TestFixture]
    public class FavoriteControllerTests
    {
        private Mock<IFavoriteService> _serviceMock;
        private FavoriteController _controller;
        private Guid _userId;

        [SetUp]
        public void Setup()
        {
            _serviceMock = new Mock<IFavoriteService>();
            _controller = new FavoriteController(_serviceMock.Object);
            _userId = Guid.NewGuid();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, _userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        // Função auxiliar para extrair propriedades de objetos anônimos nos testes
        private object GetPropertyValue(object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName)?.GetValue(obj, null);
        }

        [Test]
        public async Task ToggleFavorite_ShouldReturnOk_WhenAdded()
        {
            // Arrange
            var dto = new FavoriteRequestDto { PropertyId = Guid.NewGuid() };
            _serviceMock.Setup(s => s.ToggleFavoriteAsync(_userId, dto.PropertyId))
                        .ReturnsAsync(true);

            // Act
            var result = await _controller.ToggleFavorite(dto) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(200));
            
            var message = GetPropertyValue(result.Value, "message");
            Assert.That(message, Is.EqualTo("Propriedade adicionada aos favoritos"));
        }

        [Test]
        public async Task ToggleFavorite_ShouldReturnOk_WhenRemoved()
        {
            // Arrange
            var dto = new FavoriteRequestDto { PropertyId = Guid.NewGuid() };
            _serviceMock.Setup(s => s.ToggleFavoriteAsync(_userId, dto.PropertyId))
                        .ReturnsAsync(false);

            // Act
            var result = await _controller.ToggleFavorite(dto) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(200));

            var message = GetPropertyValue(result.Value, "message");
            Assert.That(message, Is.EqualTo("Propriedade removida dos favoritos"));
        }

        [Test]
        public async Task ToggleFavorite_ShouldReturnBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var dto = new FavoriteRequestDto { PropertyId = Guid.NewGuid() };
            _serviceMock.Setup(s => s.ToggleFavoriteAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                        .ThrowsAsync(new Exception("Imóvel não encontrado."));

            // Act
            var result = await _controller.ToggleFavorite(dto) as BadRequestObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(400));

            var message = GetPropertyValue(result.Value, "message");
            Assert.That(message, Is.EqualTo("Imóvel não encontrado."));
        }
    }
}