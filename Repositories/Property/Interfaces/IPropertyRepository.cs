using vizin.DTO.Property;
using vizin.Models;

namespace vizin.Repositories.Property.Interfaces;

public interface IPropertyRepository
{
    TbProperty Create(TbProperty property);
    List<TbProperty> SelectAllProperties();
}