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
    
    public List<TbProperty> SelectAllProperties()
    {
        return _context.TbProperties.ToList();
    }

    public async Task<TbProperty?> SelectByIdAsync(Guid propertyId)
    {
        return await _context.TbProperties
            .FirstOrDefaultAsync(p => p.Id == propertyId);
    }

    public async Task UpdateAsync(TbProperty property)
    {
        _context.TbProperties.Update(property);
        await _context.SaveChangesAsync();
    }

    public async Task<TbProperty?> GetByIdAsync(Guid propertyId)
    {
        return await _context.TbProperties
           .FirstOrDefaultAsync(p => p.Id == propertyId);
    }

}