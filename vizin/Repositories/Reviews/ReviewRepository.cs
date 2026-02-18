using Microsoft.EntityFrameworkCore;
using vizin.Models;
using vizin.Repositories.Reviews.Interfaces;

namespace vizin.Repositories.Reviews;

public class ReviewRepository : IReviewRepository
{
    private readonly PostgresContext _context;
    
    public ReviewRepository(PostgresContext context) => _context = context;
    
    public async Task<TbReview?> GetByIdAsync(Guid id)
    {
        return await _context.TbReviews.FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<TbReview?> GetByBookingIdAsync(Guid bookingId)
    {
        // Essencial para a validação de "Não avaliar 2 vezes a mesma reserva"
        return await _context.TbReviews
            .FirstOrDefaultAsync(r => r.BookingId == bookingId);
    }

    public async Task<IEnumerable<TbReview>> GetByPropertyIdAsync(Guid propertyId)
    {
        // Buscamos as avaliações navegando pela reserva para saber de qual imóvel elas são
        return await _context.TbReviews
            .Include(r => r.User) // Inclui dados do autor (nome/foto) para mostrar no Front
            .Where(r => r.Booking.PropertyId == propertyId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task CreateAsync(TbReview review)
    {
        await _context.TbReviews.AddAsync(review);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var review = await GetByIdAsync(id);
        if (review != null)
        {
            _context.TbReviews.Remove(review);
            await _context.SaveChangesAsync();
        }
    }
}