using Microsoft.EntityFrameworkCore.ChangeTracking;
using vizin.Models;
using vizin.Repositories.Property.Interfaces;
using Microsoft.EntityFrameworkCore;

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

}