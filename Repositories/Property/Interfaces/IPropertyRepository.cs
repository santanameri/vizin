using vizin.DTO.Property;
using vizin.Models;

namespace vizin.Repositories.Property.Interfaces;

public interface IPropertyRepository
{
    public TbProperty Create(TbProperty property);
    public List<TbProperty> SelectAllProperties();
}