using Moq;
using vizin.Models;
using vizin.Models.Enum;
using vizin.Repositories.Booking.Interfaces;
using vizin.Services.Booking;

namespace vizin.Tests.Mappers.Booking;
[TestFixture]
public class BookingMapperTest
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
    public void BookingMapper_ToDto_ShouldMapAllFieldsCorrectly()
    {
        // Arrange
        var property = new TbProperty { Title = "Casa de Luxo", DailyValue = 100 };
        var booking = new TbBooking 
        { 
            Id = Guid.NewGuid(), 
            CheckinDate = DateTime.UtcNow, 
            CheckoutDate = DateTime.UtcNow.AddDays(2),
            Status = (int)StatusBookingType.Criado,
            TotalCost = 200
        };

        // Act
        var dto = BookingMapper.ToDto(booking, property);

        // Assert
        Assert.Multiple(() => {
            Assert.That(dto.PropertyTitle, Is.EqualTo("Casa de Luxo"));
            Assert.That(dto.TotalNights, Is.EqualTo(2));
            Assert.That(dto.TotalCost, Is.EqualTo(200));
            Assert.That(dto.Status, Is.EqualTo(StatusBookingType.Criado));
        });
    }
}