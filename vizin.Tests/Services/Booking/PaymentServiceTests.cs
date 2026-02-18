using Moq;
using vizin.DTO.Booking;
using vizin.Models;
using vizin.Models.Enum;
using vizin.Repositories.Booking.Interfaces;
using vizin.Services.Booking.Payment;

namespace vizin.Tests.Services;

[TestFixture]
public class PaymentServiceTests
{
    private Mock<IPaymentRepository> _paymentRepoMock;
    private Mock<IBookingRepository> _bookingRepoMock;
    private PaymentService _service;
    private Guid _userId; 

    [SetUp]
    public void SetUp()
    {
        _paymentRepoMock = new Mock<IPaymentRepository>();
        _bookingRepoMock = new Mock<IBookingRepository>();
        _service = new PaymentService(_paymentRepoMock.Object, _bookingRepoMock.Object);
        _userId = Guid.NewGuid();
    }

    [Test]
public async Task ProcessPayment_Success_ShouldConfirmBooking()
{
    // Arrange
    var bookingId = Guid.NewGuid();
    var userId = Guid.NewGuid();
    
    // 1. Cartão começa com 44 para bater com o StartsWith("44") da Service
    var dto = new PaymentRequestDto { 
        CardNumber = "44123456789", 
        Method = PaymentMethodType.Credito 
    };
    
    var booking = new TbBooking 
    { 
        Id = bookingId, 
        UserId = userId, 
        TotalCost = 500, 
        Status = (int)StatusBookingType.Criado 
    };

    _bookingRepoMock.Setup(r => r.GetByIdAsync(bookingId)).ReturnsAsync(booking);

    // IMPORTANTE: Garanta que o CreateAsync não retorne null, pois a Service precisa do objeto criado
    _paymentRepoMock.Setup(r => r.CreateAsync(It.IsAny<TbPayment>()))
        .Returns(Task.CompletedTask); 

    // Act
    var result = await _service.ProcessPaymentAsync(bookingId, userId, dto);

    // Assert
    Assert.Multiple(() =>
    {
        // Se isPaymentApproved for true, o StatusPayment na Service é (int)StatusPaymentType.Aprovado
        // Se Aprovado for 2, result.Status será "2"
        Assert.That(booking.Status, Is.EqualTo((int)StatusBookingType.Confirmado), "O status da reserva deveria ter mudado para Confirmado.");
        Assert.That(result.Amount, Is.EqualTo(500), "O valor do pagamento deve ser igual ao custo da reserva.");
    });
}

    [Test]
    public async Task ProcessPayment_Declined_ShouldNotUpdateBooking()
    {
        // Arrange - Cartão terminando em "00" para simular falha
        var bookingId = Guid.NewGuid();
        var userId = _userId;
        var dto = new PaymentRequestDto { CardNumber = "123400", Method = PaymentMethodType.Credito };
        
        var booking = new TbBooking { Id = bookingId, UserId = userId, Status = (int)StatusBookingType.Criado };
        _bookingRepoMock.Setup(r => r.GetByIdAsync(bookingId)).ReturnsAsync(booking);

        // Act
        var result = await _service.ProcessPaymentAsync(bookingId, userId, dto);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(booking.Status, Is.EqualTo((int)StatusBookingType.Criado)); // Status não mudou
        _bookingRepoMock.Verify(r => r.UpdateAsync(It.IsAny<TbBooking>()), Times.Never);
    }

    [Test]
    public void ProcessPayment_WrongUser_ShouldThrowException()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var ownerId = _userId;
        var strangerId = Guid.NewGuid();
        
        var booking = new TbBooking { Id = bookingId, UserId = ownerId };
        _bookingRepoMock.Setup(r => r.GetByIdAsync(bookingId)).ReturnsAsync(booking);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => 
            await _service.ProcessPaymentAsync(bookingId, strangerId, new PaymentRequestDto()));
        
        Assert.That(ex.Message, Is.EqualTo("Usuário não autorizado para este pagamento."));
    }

    [Test]
    public void ProcessPayment_BookingNotFound_ShouldThrowException()
    {
        // Arrange
        _bookingRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((TbBooking)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => 
            await _service.ProcessPaymentAsync(Guid.NewGuid(), Guid.NewGuid(), new PaymentRequestDto()));
        
        Assert.That(ex.Message, Is.EqualTo("Reserva não encontrada."));
    }
    
    [Test]
    public async Task ProcessPayment_Declined_ShouldCreateFailedRecordButNotConfirmBooking()
    {
        // Arrange - Cartão que NÃO começa com "44"
        var bookingId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var dto = new PaymentRequestDto { CardNumber = "9999", Method = PaymentMethodType.Credito };
    
        var booking = new TbBooking { Id = bookingId, UserId = userId, Status = (int)StatusBookingType.Criado };
        _bookingRepoMock.Setup(r => r.GetByIdAsync(bookingId)).ReturnsAsync(booking);
        
        var result = await _service.ProcessPaymentAsync(bookingId, userId, dto);

        Assert.Multiple(() =>
        {
            Assert.That(booking.Status, Is.EqualTo((int)StatusBookingType.Criado), "O status da reserva NÃO deveria mudar.");
        });
    
        // Verifica que o UpdateAsync da reserva NUNCA foi chamado
        _bookingRepoMock.Verify(r => r.UpdateAsync(It.IsAny<TbBooking>()), Times.Never);
    }
}