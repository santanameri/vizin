using vizin.DTO.Favorite;

namespace vizin.Services.Favorite.Interfaces;
public interface IFavoriteService
{
    Task<bool> ToggleFavoriteAsync(Guid userId, Guid propertyId);
}