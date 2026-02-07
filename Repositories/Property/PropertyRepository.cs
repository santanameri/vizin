using Microsoft.EntityFrameworkCore.ChangeTracking;
using vizin.Models;
using vizin.Repositories.Property.Interfaces;

namespace vizin.Repositories.Property;

public class PropertyRepository : IPropertyRepository
{
    private PostgresContext _context;

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
}