using vizin.DTO.Property;
using vizin.Models;

namespace vizin.Repositories.Property.Interfaces;

public interface IPropertyRepository
{
    TbProperty Create(TbProperty property);
    List<TbProperty> SelectAllProperties();
    Task<TbProperty?> SelectByIdAsync(Guid propertyId);
    Task UpdateAsync(TbProperty property);
    Task<TbProperty?> GetByIdAsync(Guid propertyId);

    //colei embaixo
    Task<TbProperty?> GetPropertyById(Guid propertyId);
}