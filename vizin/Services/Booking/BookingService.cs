using vizin.DTO.Booking;
using vizin.Models;
using vizin.Models.Enum;
using vizin.Repositories.Booking.Interfaces;
using vizin.Repositories.Property.Interfaces;
using vizin.Services.Booking.Interfaces;

namespace vizin.Services.Booking;

public class BookingService : IBookingService
{ 
    private readonly IBookingRepository _bookingRepo;
    private readonly IPropertyRepository _propertyRepository;

    public BookingService(IBookingRepository bookingRepo)
    {
        _bookingRepo = bookingRepo;
    }
    
    public async Task<BookingResponseDto> CreateBooking(Guid userId, Guid propertyId, CreateBookingDto dto)
    {
        if (dto.CheckIn < DateTime.Today) throw new Exception("Check-in no passado.");
        if (dto.CheckOut <= dto.CheckIn) throw new Exception("Check-out deve ser após check-in.");
        
        var property = await _bookingRepo.GetPropertyWithCapacityAsync(propertyId);
        if (property == null) throw new Exception("Imóvel inexistente.");
        if (dto.GuestCount > property.Capacity) throw new Exception("Capacidade excedida.");
        
        var isOccupied = await _bookingRepo.HasConflictingBookingAsync(propertyId, dto.CheckIn, dto.CheckOut);
        if (isOccupied) throw new Exception("Já reservado para este período.");
        
        var totalNights = (dto.CheckOut.Date - dto.CheckIn.Date).Days;
        if (totalNights <=0) totalNights = 1;
        
        TbBooking newBooking = new TbBooking
        {
            UserId = userId,
            PropertyId = propertyId,
            CheckinDate = dto.CheckIn,
            CheckoutDate = dto.CheckOut,
            GuestCount = dto.GuestCount,
            TotalCost = totalNights * property.DailyValue,
            Status = (int)StatusBookingType.Criado
        };
        
        TbBooking created = await _bookingRepo.CreateAsync(newBooking);
        
        return BookingMapper.ToDto(created, property);
        
    }

    public async Task<BookingHistoryDto> GetUserBookingHistoryAsync(Guid userId, string role)
    {
        List<TbBooking> bookings;
        
        if (role == "Anfitriao") // Ajuste conforme o nome exato da sua Policy/Role
            bookings = await _bookingRepo.GetHostBookingsAsync(userId);
        else
            bookings = await _bookingRepo.GetGuestBookingsAsync(userId);

        var today = DateTime.Today;

        var history = new BookingHistoryDto
        {
            // Em andamento: data_inicio <= hoje E data_fim >= hoje
            Ongoing = bookings
                .Where(b => b.CheckinDate.Date <= today && b.CheckoutDate.Date >= today)
                .Select(b => BookingMapper.ToDto(b, b.Property))
                .ToList(),

            // Anteriores: data_fim < hoje
            Past = bookings
                .Where(b => b.CheckoutDate.Date < today)
                .Select(b => BookingMapper.ToDto(b, b.Property))
                .ToList()
        };

        return history;
    }
    
    public async Task<bool> CancelBookingAsync(Guid bookingId, Guid userId)
    {
        var booking = await _bookingRepo.GetByIdAsync(bookingId);

        if (booking == null) throw new Exception("Reserva não encontrada.");

        // Validação: Apenas o dono da reserva pode cancelar
        if (booking.UserId != userId) throw new Exception("Acesso negado.");

        // Validação de Status (3 = Cancelado, 4 = Finalizado)
        if (booking.Status == 3) throw new Exception("Reserva já está cancelada.");
        if (booking.Status == 4) throw new Exception("Não é possível cancelar uma reserva finalizada.");

        // Regra: Atualiza para status 3 (Cancelado)
        booking.Status = 3;
        booking.CancelationDate = DateTime.Now;

        await _bookingRepo.UpdateAsync(booking);
        return true;
    }
}