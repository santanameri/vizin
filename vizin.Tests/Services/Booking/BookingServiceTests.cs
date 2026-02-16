using Moq;
using vizin.DTO.Booking;
using vizin.Models;
using vizin.Models.Enum;
using vizin.Repositories.Booking.Interfaces;
using vizin.Services.Booking;

namespace vizin.Tests.Services;

[TestFixture]
public class BookingServiceTests
{
    private Mock<IBookingRepository> _bookingRepoMock;
    private BookingService _service;

    [SetUp]
    public void SetUp()
    {
        _bookingRepoMock = new Mock<IBookingRepository>();
        _service = new BookingService(_bookingRepoMock.Object);
    }

    [Test]
    public async Task CreateBooking_Success_ShouldReturnCorrectDto()
    {
        var propertyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var dto = new CreateBookingDto
        {
            CheckIn = DateTime.Today.AddDays(1),
            CheckOut = DateTime.Today.AddDays(3),
            GuestCount = 2
        };

        var property = new TbProperty
        {
            Id = propertyId,
            Capacity = 4,
            DailyValue = 100,
            Title = "Casa de Teste"
        };

        _bookingRepoMock.Setup(r => r.GetPropertyWithCapacityAsync(propertyId))
            .ReturnsAsync(property);

        _bookingRepoMock.Setup(r => r.HasConflictingBookingAsync(propertyId, dto.CheckIn, dto.CheckOut))
            .ReturnsAsync(false);

        _bookingRepoMock.Setup(r => r.CreateAsync(It.IsAny<TbBooking>()))
            .ReturnsAsync(new TbBooking { Id = Guid.NewGuid() });

        var result = await _service.CreateBooking(userId, propertyId, dto);

        Assert.Multiple(() =>
        {
            Assert.That(result.PropertyTitle, Is.EqualTo("Casa de Teste"));
            Assert.That(result.TotalNights, Is.EqualTo(2));
            Assert.That(result.TotalCost, Is.EqualTo(200));
            Assert.That(result.Status, Is.EqualTo(StatusBookingType.Criado));
        });

        _bookingRepoMock.Verify(r => r.CreateAsync(It.IsAny<TbBooking>()), Times.Once);
    }

    [Test]
    public void CreateBooking_CheckInInPast_ShouldThrowException()
    {
        var dto = new CreateBookingDto { CheckIn = DateTime.Today.AddDays(-1) };

        var ex = Assert.ThrowsAsync<Exception>(async () =>
            await _service.CreateBooking(Guid.NewGuid(), Guid.NewGuid(), dto));

        Assert.That(ex.Message, Is.EqualTo("Check-in no passado."));
    }

    [Test]
    public void CreateBooking_CapacityExceeded_ShouldThrowException()
    {

        var propertyId = Guid.NewGuid();
        var dto = new CreateBookingDto
        {
            CheckIn = DateTime.Today.AddDays(1),
            CheckOut = DateTime.Today.AddDays(2),
            GuestCount = 10
        };

        var property = new TbProperty { Id = propertyId, Capacity = 5 };

        _bookingRepoMock.Setup(r => r.GetPropertyWithCapacityAsync(propertyId))
            .ReturnsAsync(property);

        var ex = Assert.ThrowsAsync<Exception>(async () =>
            await _service.CreateBooking(Guid.NewGuid(), propertyId, dto));

        Assert.That(ex.Message, Is.EqualTo("Capacidade excedida."));
    }

    [Test]
    public void CreateBooking_WhenOccupied_ShouldThrowException()
    {

        var propertyId = Guid.NewGuid();
        var dto = new CreateBookingDto
        {
            CheckIn = DateTime.Today.AddDays(1),
            CheckOut = DateTime.Today.AddDays(2),
            GuestCount = 2
        };

        _bookingRepoMock.Setup(r => r.GetPropertyWithCapacityAsync(propertyId))
            .ReturnsAsync(new TbProperty { Capacity = 5 });

        _bookingRepoMock.Setup(r => r.HasConflictingBookingAsync(propertyId, dto.CheckIn, dto.CheckOut))
            .ReturnsAsync(true); // Simula conflito de datas

        var ex = Assert.ThrowsAsync<Exception>(async () =>
            await _service.CreateBooking(Guid.NewGuid(), propertyId, dto));

        Assert.That(ex.Message, Is.EqualTo("Já reservado para este período."));
    }

    [Test]
    public void CreateBooking_CheckOutBeforeOrEqualCheckIn_ShouldThrowException()
    {
        // Check-in dia 20 e Check-out dia 19 (inválido)
        var dto = new CreateBookingDto
        {
            CheckIn = DateTime.Today.AddDays(20),
            CheckOut = DateTime.Today.AddDays(19),
            GuestCount = 1
        };


        var ex = Assert.ThrowsAsync<Exception>(async () =>
            await _service.CreateBooking(Guid.NewGuid(), Guid.NewGuid(), dto));

        Assert.That(ex.Message, Is.EqualTo("Check-out deve ser após check-in."));
    }

    [Test]
    public void CreateBooking_PropertyNotFound_ShouldThrowException()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var dto = new CreateBookingDto
        {
            CheckIn = DateTime.Today.AddDays(1),
            CheckOut = DateTime.Today.AddDays(2),
            GuestCount = 1
        };

        // Simulando que o repositório não encontrou nada no banco
        _bookingRepoMock.Setup(r => r.GetPropertyWithCapacityAsync(propertyId))
            .ReturnsAsync((TbProperty)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () =>
            await _service.CreateBooking(Guid.NewGuid(), propertyId, dto));

        Assert.That(ex.Message, Is.EqualTo("Imóvel inexistente."));
    }
}