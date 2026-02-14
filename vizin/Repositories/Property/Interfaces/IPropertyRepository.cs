using vizin.DTO.Property;
using vizin.Models;

namespace vizin.Repositories.Property.Interfaces;

public interface IPropertyRepository
{
    TbProperty Create(TbProperty property);
    Task<List<TbProperty>> SelectAllProperties();
    Task<List<TbProperty>> SelectAllPropertiesByHost(Guid hostId);
    Task<TbProperty?> GetPropertyById(Guid propertyId);
    Task<TbProperty?> Update(Guid propertyId, TbProperty property);
}