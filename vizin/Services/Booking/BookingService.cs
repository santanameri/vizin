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
}