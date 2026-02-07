using vizin.DTO.Property;

namespace vizin.Services.Property.Interfaces;

public interface IPropertyService
{
    public List<PropertyResponseDto> GetProperties();
}