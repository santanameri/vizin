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
        _paymentRepoMock.Setup(r => r.GetByBookingIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<TbPayment>());
    }

    [Test]
public async Task ProcessPayment_Success_ShouldConfirmBooking()
{
    // Arrange
    var bookingId = Guid.NewGuid();
    var userId = _userId;
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
        Status = (int)StatusBookingType.Criado,
        CheckinDate = DateTime.UtcNow.AddDays(1)
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
        var dto = new PaymentRequestDto
        {
            CardNumber = "123400", 
            Method = PaymentMethodType.Credito
        };
        
        var booking = new TbBooking
        {
            Id = bookingId, 
            UserId = userId,
            TotalCost = 500,
            Status = (int)StatusBookingType.Criado,
            CheckinDate = DateTime.UtcNow.AddDays(1)
        };
        
        _bookingRepoMock.Setup(r => r.GetByIdAsync(bookingId)).ReturnsAsync(booking);
        
        var result = await _service.ProcessPaymentAsync(bookingId, userId, dto);

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
        var bookingId = Guid.NewGuid();
        var userId = _userId;
        var dto = new PaymentRequestDto { CardNumber = "9999", Method = PaymentMethodType.Credito };

        var booking = new TbBooking { 
            Id = bookingId, 
            UserId = userId,
            TotalCost = 500,
            Status = (int)StatusBookingType.Criado,
            CheckinDate = DateTime.UtcNow.AddDays(1)
        };
        
        _paymentRepoMock.Setup(r => r.GetByBookingIdAsync(bookingId))
            .ReturnsAsync([]);

        _bookingRepoMock.Setup(r => r.GetByIdAsync(bookingId)).ReturnsAsync(booking);
        
        var result = await _service.ProcessPaymentAsync(bookingId, userId, dto);

        Assert.Multiple(() =>
        {
            Assert.That(booking.Status, Is.EqualTo((int)StatusBookingType.Criado), "O status da reserva NÃO deveria mudar.");
        });

        _bookingRepoMock.Verify(r => r.UpdateAsync(It.IsAny<TbBooking>()), Times.Never);
    }
    
    [Test]
    public void ProcessPayment_AlreadyPaid_ShouldThrowException()
    {
        var bookingId = Guid.NewGuid();
        var userId = _userId;
    
        var booking = new TbBooking
        {
            Id = bookingId, 
            UserId = userId, 
            Status = (int)StatusBookingType.Criado
        };
    
        // Simulamos que já existe um pagamento APROVADO para este booking no banco
        var existingPayments = new List<TbPayment> 
        { 
            new TbPayment { 
                BookingId = bookingId, 
                StatusPayment = (int)StatusPaymentType.Aprovado 
            } 
        };

        _bookingRepoMock.Setup(r => r.GetByIdAsync(bookingId)).ReturnsAsync(booking);
    
        // Configuramos o mock para retornar a lista com o pagamento já aprovado
        _paymentRepoMock.Setup(r => r.GetByBookingIdAsync(bookingId)).ReturnsAsync(existingPayments);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => 
            await _service.ProcessPaymentAsync(bookingId, userId, new PaymentRequestDto()));
    
        Assert.That(ex.Message, Is.EqualTo("Reserva já paga"));
    
        // Garantimos que o sistema nem tentou atualizar ou criar nada novo
        _bookingRepoMock.Verify(r => r.UpdateAsync(It.IsAny<TbBooking>()), Times.Never);
        _paymentRepoMock.Verify(r => r.CreateAsync(It.IsAny<TbPayment>()), Times.Never);
    }
    
    [Test]
    public void ProcessPayment_InvalidCost_ShouldThrowException()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var userId = _userId;
        var booking = new TbBooking { Id = bookingId, UserId = userId, TotalCost = 0, Status = (int)StatusBookingType.Criado };
    
        _bookingRepoMock.Setup(r => r.GetByIdAsync(bookingId)).ReturnsAsync(booking);
        _paymentRepoMock.Setup(r => r.GetByBookingIdAsync(bookingId)).ReturnsAsync(new List<TbPayment>());

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => 
            await _service.ProcessPaymentAsync(bookingId, userId, new PaymentRequestDto()));
    
        Assert.That(ex.Message, Is.EqualTo("O valor da reserva é inválido para pagamento."));
    }
    
    [Test]
    public void ProcessPayment_PastDate_ShouldThrowException()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var userId = _userId;
        // Reserva para ontem
        var booking = new TbBooking 
        { 
            Id = bookingId, 
            UserId = userId, 
            CheckinDate = DateTime.UtcNow.AddDays(-1), 
            Status = (int)StatusBookingType.Criado,
            TotalCost = 100
        };
    
        _bookingRepoMock.Setup(r => r.GetByIdAsync(bookingId)).ReturnsAsync(booking);
        _paymentRepoMock.Setup(r => r.GetByBookingIdAsync(bookingId)).ReturnsAsync(new List<TbPayment>());

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => 
            await _service.ProcessPaymentAsync(bookingId, userId, new PaymentRequestDto()));
    
        Assert.That(ex.Message, Is.EqualTo("Não é possível pagar uma reserva para a data de hoje ou datas passadas."));
    }
}