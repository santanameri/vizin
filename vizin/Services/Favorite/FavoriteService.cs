using System;
using System.Threading.Tasks;
using vizin.Models;
using vizin.Repositories.Property.Interfaces;
using vizin.Services.Favorite.Interfaces;
public class FavoriteService : IFavoriteService
{
    private readonly IFavoriteRepository _favoriteRepo;
    private readonly IPropertyRepository _propertyRepo; // Precisamos injetar para validar o imóvel

    public FavoriteService(IFavoriteRepository favoriteRepo, IPropertyRepository propertyRepo)
    {
        _favoriteRepo = favoriteRepo;
        _propertyRepo = propertyRepo;
    }

    public async Task<bool> ToggleFavoriteAsync(Guid userId, Guid propertyId)
    {
        var propertyExists = await _propertyRepo.GetByIdAsync(propertyId);
        if (propertyExists == null) throw new Exception("Imóvel não encontrado.");

        var alreadyFavorite = await _favoriteRepo.IsFavoriteAsync(userId, propertyId);

        if (alreadyFavorite)
        {
            await _favoriteRepo.RemoveFavoriteAsync(userId, propertyId);
            return false; // Indica que foi removido
        }
        else
        {
            var favorite = new TbFavorite
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PropertyId = propertyId
            };
            await _favoriteRepo.AddFavoriteAsync(favorite);
            return true; // Indica que foi adicionado
        }
    }
}