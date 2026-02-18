using vizin.DTO.ReviewRequestDto;
using vizin.Models;
using vizin.Models.Enum;
using vizin.Repositories.Booking.Interfaces;
using vizin.Repositories.Reviews.Interfaces;
using vizin.Services.Review.Interfaces;

namespace vizin.Services.Review;

public class ReviewService: IReviewService
{
    private readonly IReviewRepository _reviewRepo;
    private readonly IBookingRepository _bookingRepo;

    public ReviewService(IReviewRepository reviewRepo, IBookingRepository bookingRepo)
    {
        _reviewRepo = reviewRepo;
        _bookingRepo = bookingRepo;
    }

    // 1. Hóspede avalia o Imóvel
    public async Task CreateBookingReviewAsync(Guid authorId, Guid bookingId, ReviewRequestDto dto)
    {
        var booking = await _bookingRepo.GetByIdAsync(bookingId);

        if (booking == null) throw new Exception("Reserva não encontrada.");

        // 2. Valida se a reserva já terminou e se o autor faz parte dela
        // (Apenas o hóspede avalia o imóvel/anfitrião nessa estrutura)
        if (booking.UserId != authorId)
            throw new Exception("Você não tem permissão para avaliar esta reserva.");

        if (booking.Status != (int)StatusBookingType.Finalizado)
            throw new Exception("Você só pode avaliar uma reserva após a conclusão.");

        // 3. Valida se já existe uma avaliação para essa reserva (Idempotência)
        var existingReview = await _reviewRepo.GetByBookingIdAsync(bookingId);
        if (existingReview != null)
            throw new Exception("Esta reserva já foi avaliada.");

        // 4. Cria a entidade conforme sua classe TbReview
        var review = new TbReview
        {
            Id = Guid.NewGuid(),
            UserId = authorId,
            BookingId = bookingId,
            Note = dto.Stars, 
            Comment = dto.Comment,
            CreatedAt = DateTime.UtcNow
        };

        await _reviewRepo.CreateAsync(review);
    }
    
    // 3. Remover Avaliação
    public async Task DeleteReviewAsync(Guid reviewId, Guid userId)
    {
        var review = await _reviewRepo.GetByIdAsync(reviewId);
        if (review == null) throw new Exception("Avaliação não encontrada.");
        
        if (review.UserId != userId)
            throw new Exception("Apenas o autor pode remover a avaliação.");

        await _reviewRepo.DeleteAsync(reviewId);
    }
}