using Moq;
using NUnit.Framework;
using vizin.Models;
using vizin.Repositories.Property.Interfaces;
using vizin.Services.Favorite;

namespace vizin.Tests.Services.Favorite
{
    [TestFixture]
    public class FavoriteServiceTests
    {
        private Mock<IFavoriteRepository> _favoriteRepoMock;
        private Mock<IPropertyRepository> _propertyRepoMock;
        private FavoriteService _service;
        private Guid _userId;
        private Guid _propertyId;

        [SetUp]
        public void Setup()
        {
            _favoriteRepoMock = new Mock<IFavoriteRepository>();
            _propertyRepoMock = new Mock<IPropertyRepository>();
            _service = new FavoriteService(_favoriteRepoMock.Object, _propertyRepoMock.Object);
            
            _userId = Guid.NewGuid();
            _propertyId = Guid.NewGuid();
        }

        [Test]
        public async Task ToggleFavorite_ShouldAddFavorite_WhenItDoesNotExist()
        {
            // Arrange
            _propertyRepoMock.Setup(r => r.GetByIdAsync(_propertyId))
                             .ReturnsAsync(new TbProperty()); // Imóvel existe

            _favoriteRepoMock.Setup(r => r.IsFavoriteAsync(_userId, _propertyId))
                             .ReturnsAsync(false); // Não é favorito ainda

            // Act
            var result = await _service.ToggleFavoriteAsync(_userId, _propertyId);

            // Assert
            Assert.That(result, Is.True); // Retorna true (adicionado)
            _favoriteRepoMock.Verify(r => r.AddFavoriteAsync(It.IsAny<TbFavorite>()), Times.Once);
            _favoriteRepoMock.Verify(r => r.RemoveFavoriteAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public async Task ToggleFavorite_ShouldRemoveFavorite_WhenItAlreadyExists()
        {
            // Arrange
            _propertyRepoMock.Setup(r => r.GetByIdAsync(_propertyId))
                             .ReturnsAsync(new TbProperty());

            _favoriteRepoMock.Setup(r => r.IsFavoriteAsync(_userId, _propertyId))
                             .ReturnsAsync(true); // Já é favorito

            // Act
            var result = await _service.ToggleFavoriteAsync(_userId, _propertyId);

            // Assert
            Assert.That(result, Is.False); // Retorna false (removido)
            _favoriteRepoMock.Verify(r => r.RemoveFavoriteAsync(_userId, _propertyId), Times.Once);
            _favoriteRepoMock.Verify(r => r.AddFavoriteAsync(It.IsAny<TbFavorite>()), Times.Never);
        }

        [Test]
        public void ToggleFavorite_ShouldThrowException_WhenPropertyNotFound()
        {
            // Arrange
            _propertyRepoMock.Setup(r => r.GetByIdAsync(_propertyId))
                             .ReturnsAsync((TbProperty)null); // Imóvel não encontrado

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () => 
                await _service.ToggleFavoriteAsync(_userId, _propertyId));
            
            Assert.That(ex.Message, Is.EqualTo("Imóvel não encontrado."));
        }
    }
}