using vizin.Models;

public interface IFavoriteRepository
{
    Task AddFavoriteAsync(TbFavorite favorite);
    Task RemoveFavoriteAsync(Guid userId, Guid propertyId);
    Task<bool> IsFavoriteAsync(Guid userId, Guid propertyId);
    Task<List<TbFavorite>> GetUserFavoritesAsync(Guid userId);
}