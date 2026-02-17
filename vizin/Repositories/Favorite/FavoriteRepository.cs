using Microsoft.EntityFrameworkCore;
using vizin.Models;

public class FavoriteRepository : IFavoriteRepository
{
    private readonly PostgresContext _context;

    public FavoriteRepository(PostgresContext context) => _context = context;

    public async Task AddFavoriteAsync(TbFavorite favorite)
    {
        await _context.TbFavorites.AddAsync(favorite);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveFavoriteAsync(Guid userId, Guid propertyId)
    {
        var favorite = await _context.TbFavorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.PropertyId == propertyId);
        
        if (favorite != null)
        {
            _context.TbFavorites.Remove(favorite);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> IsFavoriteAsync(Guid userId, Guid propertyId)
    {
        return await _context.TbFavorites.AnyAsync(f => f.UserId == userId && f.PropertyId == propertyId);
    }

    public async Task<List<TbFavorite>> GetUserFavoritesAsync(Guid userId)
    {
        return await _context.TbFavorites
            .Include(f => f.Property)
            .Where(f => f.UserId == userId)
            .ToListAsync();
    }
}