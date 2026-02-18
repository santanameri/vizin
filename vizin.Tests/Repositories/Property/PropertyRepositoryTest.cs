using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using vizin.Models;
using vizin.Repositories;
using vizin.Repositories.Property;
using vizin.DTO.Property;
using MockQueryable.NSubstitute;
using NSubstitute;


namespace vizin.Tests.Repositories
{
    [TestFixture]
    public class PropertyRepositoryTests
    {
        private PostgresContext _context;
        private PropertyRepository _repository;

        [SetUp]
        public void Setup()
        {
            // 1. Criamos as opções para o InMemory
            var options = new DbContextOptionsBuilder<PostgresContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            // 2. Instanciamos o Contexto passando essas opções
            _context = new PostgresContext(options);

            // IMPORTANTE: Se o seu PostgresContext tiver um construtor padrão, 
            // o EF pode tentar usar o OnConfiguring. 
            // O ideal é que o PostgresContext aceite DbContextOptions no construtor.

            _repository = new PropertyRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task AddAmenityAsync_ShouldAddAmenity_WhenValidAndNotDuplicate()
        {
            // Arrange
            var property = new TbProperty 
            { 
                Id = Guid.NewGuid(), 
                Title = "Casa de Praia",              // Required
                FullAddress = "Avenida Beira Mar, 10", // O CAMPO QUE FALTAVA!
                Amenities = new List<TbAmenity>() 
            };
            
            var amenity = new TbAmenity 
            { 
                Id = Guid.NewGuid(), 
                Name = "WiFi" // Required
            };
            
            _context.TbProperties.Add(property);
            _context.TbAmenities.Add(amenity);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.AddAmenityAsync(amenity.Id, property.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Amenities.Count, Is.EqualTo(1));
            Assert.That(result.Amenities.First().Id, Is.EqualTo(amenity.Id));
        }

        [TestCase(true, false, Description = "Property exists, Amenity null")]
        [TestCase(false, true, Description = "Property null, Amenity exists")]
        public async Task AddAmenityAsync_ShouldReturnNull_WhenPropertyOrAmenityNotFound(bool propertyExists, bool amenityExists)
        {
            // Arrange
            var propertyId = Guid.NewGuid();
            var amenityId = Guid.NewGuid();

            if (propertyExists)
            {
                _context.TbProperties.Add(new TbProperty 
                { 
                    Id = propertyId, 
                    Title = "Casa de Teste",       // Required
                    FullAddress = "Rua Teste, 10", // Required
                    Amenities = new List<TbAmenity>() 
                });
            }

            if (amenityExists)
            {
                _context.TbAmenities.Add(new TbAmenity 
                { 
                    Id = amenityId, 
                    Name = "WiFi" // AGORA COM O CAMPO REQUIRED!
                });
            }

            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.AddAmenityAsync(amenityId, propertyId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task AddAmenityAsync_ShouldThrowException_WhenAmenityIsDuplicate()
        {
            // Arrange
            var amenityId = Guid.NewGuid();
            var amenity = new TbAmenity { Id = amenityId, Name = "Pool" };
            
            // Adicionamos os campos obrigatórios (FullAddress e Title)
            var property = new TbProperty 
            { 
                Id = Guid.NewGuid(), 
                Title = "Propriedade de Teste",       // Campo obrigatório
                FullAddress = "Rua dos Testes, 123", // Campo obrigatório
                Amenities = new List<TbAmenity> { amenity } 
            };

            _context.TbProperties.Add(property);
            _context.TbAmenities.Add(amenity);
            await _context.SaveChangesAsync();

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await _repository.AddAmenityAsync(amenityId, property.Id));

            Assert.That(ex.Message, Is.EqualTo("Comodidade duplicada!"));
        }

        [Test]
        public async Task SearchWithFiltersAsync_ShouldReturnAll_WhenFiltersAreEmpty()
        {
            // Arrange
            _context.TbProperties.AddRange(new List<TbProperty>
            {
                new TbProperty { Id = Guid.NewGuid(), Title = "A", FullAddress = "Estado1, Cidade1, Bairro1" },
                new TbProperty { Id = Guid.NewGuid(), Title = "B", FullAddress = "Estado2, Cidade2, Bairro2" }
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.SearchWithFiltersAsync(new PropertyFilterParams());

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
        }

    }
}