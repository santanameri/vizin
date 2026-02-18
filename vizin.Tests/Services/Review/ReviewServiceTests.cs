using Moq;
using vizin.DTO.ReviewRequestDto;
using vizin.Models;
using vizin.Repositories.Booking.Interfaces;
using vizin.Repositories.Reviews.Interfaces;
using vizin.Services.Review;

namespace vizin.Tests.Services.Review;

[TestFixture]
public class ReviewServiceTests
{
    private Mock<IReviewRepository> _reviewRepoMock;
    private Mock<IBookingRepository> _bookingRepoMock;
    private ReviewService _service;

    [SetUp]
    public void SetUp()
    {
        _reviewRepoMock = new Mock<IReviewRepository>();
        _bookingRepoMock = new Mock<IBookingRepository>();
        _service = new ReviewService(_reviewRepoMock.Object, _bookingRepoMock.Object);
    }

    [Test]
    public void CreateReview_UserNotOwnerOfBooking_ShouldThrowException()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var booking = new TbBooking { Id = Guid.NewGuid(), UserId = Guid.NewGuid() }; // Outro usuário
        _bookingRepoMock.Setup(r => r.GetByIdAsync(booking.Id)).ReturnsAsync(booking);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => 
            await _service.CreateBookingReviewAsync(authorId, booking.Id, new ReviewRequestDto()));
        
        Assert.That(ex.Message, Is.EqualTo("Você não tem permissão para avaliar esta reserva."));
    }

    [Test]
    public void CreateReview_BookingNotFinished_ShouldThrowException()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var booking = new TbBooking { Id = Guid.NewGuid(), UserId = authorId, Status = 1 }; // Status 1 = Criado
        _bookingRepoMock.Setup(r => r.GetByIdAsync(booking.Id)).ReturnsAsync(booking);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => 
            await _service.CreateBookingReviewAsync(authorId, booking.Id, new ReviewRequestDto()));
        
        Assert.That(ex.Message, Is.EqualTo("Você só pode avaliar uma reserva após a conclusão."));
    }

    [Test]
    public void CreateReview_AlreadyReviewed_ShouldThrowException()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        var booking = new TbBooking { Id = bookingId, UserId = authorId, Status = 4 }; // Status 4 = Finalizado
        
        _bookingRepoMock.Setup(r => r.GetByIdAsync(bookingId)).ReturnsAsync(booking);
        // Simulando que já existe uma review
        _reviewRepoMock.Setup(r => r.GetByBookingIdAsync(bookingId)).ReturnsAsync(new TbReview());

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => 
            await _service.CreateBookingReviewAsync(authorId, bookingId, new ReviewRequestDto()));
        
        Assert.That(ex.Message, Is.EqualTo("Esta reserva já foi avaliada."));
    }
}