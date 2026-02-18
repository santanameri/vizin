using System.Text;
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
    private Guid _userId; //esse

    [SetUp]
    public void SetUp()
    {
        _bookingRepoMock = new Mock<IBookingRepository>();
        _service = new BookingService(_bookingRepoMock.Object);
        _userId = Guid.NewGuid(); //esse
    }

    [Test]
    public async Task CreateBooking_Success_ShouldReturnCorrectDto()
    {
        var propertyId = Guid.NewGuid();
        var userId = _userId;
        var checkIn = DateTime.UtcNow.AddDays(1).Date;
        var checkOut = DateTime.UtcNow.AddDays(3).Date;
        var dto = new CreateBookingDto
        {
            CheckIn = checkIn, 
            CheckOut = checkOut,
            GuestCount = 2
        };

        var property = new TbProperty
        {
            Id = propertyId,
            Capacity = 4,
            DailyValue = 100,
            Title = "Casa de Teste"
        };
        
        var createdFromDb = new TbBooking 
        { 
            Id = Guid.NewGuid(),
            PropertyId = propertyId,
            UserId = userId,
            CheckinDate = checkIn,
            CheckoutDate = checkOut,
            TotalCost = 200, 
            Status = (int)StatusBookingType.Criado
        };

        _bookingRepoMock.Setup(r => r.GetPropertyWithCapacityAsync(propertyId))
            .ReturnsAsync(property);
            
        _bookingRepoMock.Setup(r => r.HasConflictingBookingAsync(propertyId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(false);
        
        _bookingRepoMock.Setup(r => r.CreateAsync(It.IsAny<TbBooking>()))
            .ReturnsAsync(createdFromDb);
        
        var result = await _service.CreateBooking(userId, propertyId, dto);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.TotalNights, Is.EqualTo(2), "As noites calculadas devem ser 2.");
            Assert.That(result.TotalCost, Is.EqualTo(200), "O custo total deve ser 200 (2 * 100).");
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

    [Test]
    public async Task GetUserBookingHistoryAsync_WhenRoleIsAnfitriao_ShouldCallHostRepository()
    {
        // Arrange
        var role = "Anfitriao";
        _bookingRepoMock.Setup(r => r.GetHostBookingsAsync(_userId))
            .ReturnsAsync(new List<TbBooking>());

        // Act
        await _service.GetUserBookingHistoryAsync(_userId, role);

        // Assert
        _bookingRepoMock.Verify(r => r.GetHostBookingsAsync(_userId), Times.Once);
        _bookingRepoMock.Verify(r => r.GetGuestBookingsAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task GetUserBookingHistoryAsync_WhenRoleIsHospede_ShouldCallGuestRepository()
    {
        // Arrange
        var role = "Hospede";
        _bookingRepoMock.Setup(r => r.GetGuestBookingsAsync(_userId))
            .ReturnsAsync(new List<TbBooking>());

        // Act
        await _service.GetUserBookingHistoryAsync(_userId, role);

        // Assert
        _bookingRepoMock.Verify(r => r.GetGuestBookingsAsync(_userId), Times.Once);
        _bookingRepoMock.Verify(r => r.GetHostBookingsAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task GetUserBookingHistoryAsync_ShouldDistributeBookingsCorrectlyByDate()
    {
        // Arrange
        var today = DateTime.Today;
        var property = new TbProperty { Title = "Imóvel Teste", DailyValue = 100 };
        
        var bookings = new List<TbBooking>
        {
            // Reserva Em Andamento (Iniciou ontem, termina amanhã)
            new TbBooking { 
                Id = Guid.NewGuid(), 
                CheckinDate = today.AddDays(-1), 
                CheckoutDate = today.AddDays(1), 
                Property = property,
                Status = 1
            },
            // Reserva Anterior (Terminou ontem)
            new TbBooking { 
                Id = Guid.NewGuid(), 
                CheckinDate = today.AddDays(-5), 
                CheckoutDate = today.AddDays(-1), 
                Property = property,
                Status = 1
            },
            // Reserva Futura (Não deve aparecer em 'Past', mas sim em 'Ongoing' se o checkin for hoje)
            new TbBooking { 
                Id = Guid.NewGuid(), 
                CheckinDate = today, 
                CheckoutDate = today.AddDays(2), 
                Property = property,
                Status = 1
            }
        };

        _bookingRepoMock.Setup(r => r.GetGuestBookingsAsync(_userId))
            .ReturnsAsync(bookings);

        // Act
        var result = await _service.GetUserBookingHistoryAsync(_userId, "Hospede");

        // Assert
        Assert.Multiple(() =>
        {
            // Esperamos 2 reservas em Ongoing (a que começou ontem e a que começa hoje)
            Assert.That(result.Ongoing.Count, Is.EqualTo(2), "Deveria haver 2 reservas em andamento");
            // Esperamos 1 reserva em Past (a que terminou ontem)
            Assert.That(result.Past.Count, Is.EqualTo(1), "Deveria haver 1 reserva anterior");
            
            Assert.That(result.Ongoing.Any(b => b.CheckOut < today), Is.False, "Nenhuma reserva em Ongoing deveria ter terminado antes de hoje");
            Assert.That(result.Past.All(b => b.CheckOut < today), Is.True, "Todas as reservas em Past devem ter data de fim menor que hoje");
        });
    }

    //aqui
    [Test]
    public void CancelBookingAsync_DeveLancarExcecao_QuandoReservaNaoEncontrada()
    {
        // Arrange
        var bookingId = Guid.NewGuid();

        _bookingRepoMock
            .Setup(r => r.GetByIdAsync(bookingId))
            .Returns(Task.FromResult<TbBooking?>(null));

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(() =>
            _service.CancelBookingAsync(bookingId, _userId));

        Assert.That(ex!.Message, Is.EqualTo("Reserva não encontrada."));
    }

    [Test]
    public void CancelBookingAsync_DeveLancarExcecao_QuandoUsuarioNaoForDono()
    {
        // Arrange
        var bookingId = Guid.NewGuid();

        var booking = new TbBooking
        {
            Id = bookingId,
            UserId = Guid.NewGuid(),
            Status = 1
        };

        _bookingRepoMock
            .Setup(r => r.GetByIdAsync(bookingId))
            .ReturnsAsync(booking);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(() =>
            _service.CancelBookingAsync(bookingId, _userId));

        Assert.That(ex!.Message, Is.EqualTo("Acesso negado."));
    }

    [Test]
    public void CancelBookingAsync_DeveLancarExcecao_QuandoReservaJaCancelada()
    {
        // Arrange
        var bookingId = Guid.NewGuid();

        var booking = new TbBooking
        {
            Id = bookingId,
            UserId = _userId,
            Status = 3
        };

        _bookingRepoMock
            .Setup(r => r.GetByIdAsync(bookingId))
            .ReturnsAsync(booking);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(() =>
            _service.CancelBookingAsync(bookingId, _userId));

        Assert.That(ex!.Message, Is.EqualTo("Reserva já está cancelada."));
    }

    [Test]
    public void CancelBookingAsync_DeveLancarExcecao_QuandoReservaFinalizada()
    {
        // Arrange
        var bookingId = Guid.NewGuid();

        var booking = new TbBooking
        {
            Id = bookingId,
            UserId = _userId,
            Status = 4
        };

        _bookingRepoMock
            .Setup(r => r.GetByIdAsync(bookingId))
            .ReturnsAsync(booking);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(() =>
            _service.CancelBookingAsync(bookingId, _userId));

        Assert.That(ex!.Message, Is.EqualTo("Não é possível cancelar uma reserva finalizada."));
    }

    [Test]
    public async Task CancelBookingAsync_DeveCancelarReserva_QuandoDadosValidos()
    {
        // Arrange
        var bookingId = Guid.NewGuid();

        var booking = new TbBooking
        {
            Id = bookingId,
            UserId = _userId,
            Status = 1
        };

        _bookingRepoMock
            .Setup(r => r.GetByIdAsync(bookingId))
            .ReturnsAsync(booking);

        _bookingRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<TbBooking>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CancelBookingAsync(bookingId, _userId);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(booking.Status, Is.EqualTo(3));
        Assert.That(booking.CancelationDate, Is.Not.Null);

        _bookingRepoMock.Verify(r => r.UpdateAsync(booking), Times.Once);
    }
    [Test]
    public async Task GenerateHostReport_Success_ShouldReturnValidCsvContent()
    {
        // Arrange
        var hostId = Guid.NewGuid();
        var bookings = new List<TbBooking>
        {
            new TbBooking 
            { 
                TotalCost = 500, 
                CheckinDate = new DateTime(2026, 3, 1),
                CheckoutDate = new DateTime(2026, 3, 5),
                Property = new TbProperty { Title = "Casa de Praia" },
                User = new TbUser { Name = "Lis Isabelle" },
                Status = 4 // Finalizado
            }
        };

        _bookingRepoMock.Setup(r => r.GetBookingsByHostIdAsync(hostId)).ReturnsAsync(bookings);

        // Act
        var result = await _service.GenerateHostReportAsync(hostId);
        var csvString = Encoding.UTF8.GetString(result);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(csvString, Does.Contain("Imovel;Hospede;CheckIn;CheckOut;Valor;Status")); // Cabeçalho
            Assert.That(csvString, Does.Contain("Casa de Praia")); // Dados
            Assert.That(csvString, Does.Contain("Lis Isabelle"));
            Assert.That(csvString, Does.Contain("500"));
        });
    }

}