using Microsoft.EntityFrameworkCore;
using vizin.Models;
using vizin.Repositories.Booking.Interfaces;

namespace vizin.Repositories.Booking.Payment;

public class PaymentRepository : IPaymentRepository
{
    private readonly PostgresContext _context;
    public PaymentRepository(PostgresContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(TbPayment payment)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            await _context.TbPayments.AddAsync(payment);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<TbPayment>> GetByBookingIdAsync(Guid bookingId)
    {
        return await _context.TbPayments
            .Where(p => p.BookingId == bookingId)
            .AsNoTracking()
            .ToListAsync();
    }
}