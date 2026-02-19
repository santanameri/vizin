using vizin.DTO.ReviewRequestDto;

namespace vizin.Services.Review.Interfaces;

public interface IReviewService
{
    Task<Guid> CreateBookingReviewAsync(Guid authorId, Guid bookingId, ReviewRequestDto dto);
    Task DeleteReviewAsync(Guid reviewId, Guid userId);
}