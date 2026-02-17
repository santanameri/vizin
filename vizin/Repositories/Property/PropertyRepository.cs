using Microsoft.EntityFrameworkCore.ChangeTracking;
using vizin.Models;
using vizin.Repositories.Property.Interfaces;
using Microsoft.EntityFrameworkCore;
using vizin.DTO.Property;

namespace vizin.Repositories.Property;

public class PropertyRepository : IPropertyRepository
{
    private readonly PostgresContext _context;

    public PropertyRepository(PostgresContext context)
    {
        _context = context;
    }

    public async Task<TbProperty> Create(TbProperty property)
    {
        await _context.TbProperties.AddAsync(property);
        await _context.SaveChangesAsync();
        await _context.Entry(property)
            .Collection(p => p.Amenities)
            .LoadAsync();

        return property;
    }
    
    public async Task<List<TbProperty>> SelectAllProperties()
    {
        return await _context.TbProperties
            .Include(p => p.Amenities)
            .ToListAsync();
            
    }
    
    public async Task<List<TbProperty>> SelectAllPropertiesByHost(Guid hostId)
    {
        
        return await _context.TbProperties
            .Where(p => p.UserId == hostId)
            .Include(p => p.Amenities)
            .ToListAsync();
    }

    public async Task<TbProperty?> GetPropertyById(Guid propertyId)
    {
        return await _context.TbProperties.FirstOrDefaultAsync(p => p.Id == propertyId);
    }

    public async Task<TbProperty?> Update(Guid propertyId, TbProperty property)
    {
        var existingProperty = await GetPropertyById(propertyId);
        
        if (existingProperty == null)
            return null;
        
        _context.Entry(existingProperty).CurrentValues.SetValues(property);
        await _context.SaveChangesAsync();
        await _context.Entry(existingProperty)
            .Collection(p => p.Amenities)
            .LoadAsync();
        
        return existingProperty;
    }
    
    public async Task<TbProperty> PatchAsync(TbProperty property)
    {
        _context.TbProperties.Update(property);
        await _context.SaveChangesAsync();
        await _context.Entry(property)
            .Collection(p => p.Amenities)
            .LoadAsync();

        return property;
    }

    public async Task<List<TbAmenity>> SelectAllAmenity()
    {
        return await _context.TbAmenities.ToListAsync();
    }

    public async Task<TbAmenity?> GetAmenityById(Guid amenityId)
    {
        return await _context.TbAmenities.FirstOrDefaultAsync(a => a.Id == amenityId);
    }

    public async Task<TbProperty?> AddAmenityAsync(Guid amenityId, Guid propertyId)
    {
        var property = await _context.TbProperties
            .Include(p => p.Amenities)
            .FirstOrDefaultAsync(p => p.Id == propertyId);
        
        var amenity = await _context.TbAmenities
            .FirstOrDefaultAsync(a => a.Id == amenityId);

        if (property == null || amenity == null)
            return null;

        if (!property.Amenities.Any(a => a.Id == amenityId))
        {
            property.Amenities.Add(amenity);
            await _context.SaveChangesAsync();
        }
        else
        {
            throw new InvalidOperationException("Comodidade duplicada!");
        }
        
        await _context.Entry(property)
            .Collection(p => p.Amenities)
            .LoadAsync();

        return property;
    }

    public async Task<List<TbProperty>> SearchWithFiltersAsync(PropertyFilterParams filters)
    {
        var query = _context.TbProperties
            .Include(p => p.Amenities)
            .AsNoTracking()
            .AsQueryable();
        
        if (!string.IsNullOrEmpty(filters.Estado))
            query = query.Where(p => EF.Functions.ILike(p.FullAddress, $"%{filters.Estado}%"));

        if (!string.IsNullOrEmpty(filters.Cidade))
            query = query.Where(p => EF.Functions.ILike(p.FullAddress, $"{filters.Cidade}%"));

        if (!string.IsNullOrEmpty(filters.Bairro))
            query = query.Where(p => EF.Functions.ILike(p.FullAddress,  $"{filters.Bairro}%"));
        
        return await query.ToListAsync();
    }

    public async Task<TbProperty?> RemoveAmenityAsync(Guid amenityId, Guid propertyId)
    {
        var property = await _context.TbProperties
            .Include(p => p.Amenities)
            .FirstOrDefaultAsync(p => p.Id == propertyId);

        if (property == null) return null;

        var amenity = property.Amenities.FirstOrDefault(a => a.Id == amenityId);

        if (amenity != null)
        {
            property.Amenities.Remove(amenity);
            await _context.SaveChangesAsync();
        }

        return property;
    }

    public async Task<List<TbProperty>> GetPropertiesByAmenitiesAsync(List<Guid> amenityIds, bool matchAll)
    {
        var query = _context.TbProperties
            .Include(p => p.Amenities)
            .AsQueryable();

        if (matchAll)
        {
            // Lógica: HAVING COUNT(pa.amenity_id) = total selecionado
            query = query.Where(p => p.Amenities
                .Count(a => amenityIds.Contains(a.Id)) == amenityIds.Count);
        }
        else
        {
            // Lógica: WHERE pa.amenity_id IN (...)
            query = query.Where(p => p.Amenities
                .Any(a => amenityIds.Contains(a.Id)));
        }

        return await query.ToListAsync();
    }
    
    public async Task<List<TbProperty>> GetAvailablePropertiesAsync(DateTime checkIn, DateTime checkOut)
    {
        var utcCheckIn = DateTime.SpecifyKind(checkIn, DateTimeKind.Utc);
        var utcCheckOut = DateTime.SpecifyKind(checkOut, DateTimeKind.Utc);
        
        return await _context.TbProperties
            .FromSqlInterpolated($@"
            SELECT DISTINCT p.* FROM sistema_locacao.tb_property p
            LEFT JOIN sistema_locacao.tb_booking b ON p.id = b.property_id 
                AND b.status != 3
                AND {utcCheckIn} < b.checkout_date 
                AND {utcCheckOut} > b.checkin_date
            WHERE b.id IS NULL
        ")
            .Include(p => p.Amenities)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<TbProperty?> GetByIdAsync(Guid id)
    {
        return await _context.TbProperties.FirstOrDefaultAsync(p => p.Id == id);
    }
}