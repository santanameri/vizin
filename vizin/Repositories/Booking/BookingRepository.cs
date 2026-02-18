using Microsoft.EntityFrameworkCore;
using vizin.Models;
using vizin.Models.Enum;
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

    public async Task<List<TbBooking>> GetHostBookingsAsync(Guid hostId)
{
    return await _context.TbBookings
        .Include(b => b.Property)
        .Where(b => b.Property.UserId == hostId) // O imóvel pertence ao anfitrião
        .OrderByDescending(b => b.CheckinDate)
        .ToListAsync();
}

    public async Task<List<TbBooking>> GetGuestBookingsAsync(Guid guestId)
    {
        return await _context.TbBookings
            .Include(b => b.Property)
            .Where(b => b.UserId == guestId) // A reserva foi feita por este hóspede
            .OrderByDescending(b => b.CheckinDate)
            .ToListAsync();
    }
    
    public async Task<TbBooking?> GetByIdAsync(Guid id)
    {
        return await _context.TbBookings
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task UpdateAsync(TbBooking booking)
    {
        _context.Entry(booking).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
    
    public async Task<IEnumerable<TbBooking>> GetBookingsByHostIdAsync(Guid hostId)
    {
        return await _context.TbBookings
            .Include(b => b.User)        // Traz os dados do Hóspede (nome, etc)
            .Include(b => b.Property)    // Traz os dados do Imóvel (título, preço)
            .Where(b => b.Property.UserId == hostId) // O filtro é pelo dono do imóvel
            .OrderByDescending(b => b.CheckinDate)   // Reservas mais recentes primeiro
            .ToListAsync();
    }
}