using vizin.Models;

namespace vizin.Repositories.Reviews.Interfaces;

public interface IReviewRepository
{
    Task<TbReview?> GetByIdAsync(Guid id);
    Task<TbReview?> GetByBookingIdAsync(Guid bookingId);
    Task<IEnumerable<TbReview>> GetByPropertyIdAsync(Guid propertyId);
    Task CreateAsync(TbReview review);
    Task DeleteAsync(Guid id);
}