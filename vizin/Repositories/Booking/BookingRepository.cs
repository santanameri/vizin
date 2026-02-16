using Microsoft.EntityFrameworkCore;
using vizin.Models;
using vizin.Repositories.Booking.Interfaces;
using vizin.Repositories.Property.Interfaces;

namespace vizin.Repositories.Booking;

public class BookingRepository : IBookingRepository
{
    private readonly PostgresContext _context;

    public BookingRepository(PostgresContext context)
    {
        _context = context;
    }
    public async Task<TbProperty?> GetPropertyWithCapacityAsync(Guid propertyId)
    {
        return await _context.TbProperties
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == propertyId);
    }
    
    // conflito de datas
    public async Task<bool> HasConflictingBookingAsync(Guid propertyId, DateTime checkIn, DateTime checkOut)
    {
        return await _context.TbBookings
            .AnyAsync(b => b.PropertyId == propertyId &&
                           b.Status != 3 && // ignora canceladas
                           checkIn < b.CheckoutDate && 
                           checkOut > b.CheckinDate);
    }

    public async Task<TbBooking> CreateAsync(TbBooking booking)
    {
        await _context.TbBookings.AddAsync(booking);
        await _context.SaveChangesAsync();
        return booking;
    }
}