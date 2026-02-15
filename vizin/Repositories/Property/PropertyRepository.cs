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

    public TbProperty Create(TbProperty property)
    {
        _context.TbProperties.Add(property);
        _context.SaveChanges();
        return property;
    }
    
    public async Task<List<TbProperty>> SelectAllProperties()
    {
        return await _context.TbProperties.ToListAsync();
    }
    
    public async Task<List<TbProperty>> SelectAllPropertiesByHost(Guid hostId)
    {
        
        return await _context.TbProperties.Where(p => p.UserId == hostId).ToListAsync();
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
        
        return existingProperty;
    }
    
    public async Task PatchAsync(TbProperty property)
    {
        _context.TbProperties.Update(property);
        await _context.SaveChangesAsync();
    }
}