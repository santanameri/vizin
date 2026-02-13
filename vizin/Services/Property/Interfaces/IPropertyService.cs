using vizin.DTO.Property;

namespace vizin.Services.Property.Interfaces;

public interface IPropertyService
{
    Task<PropertyResponseDto> CreateProperty(PropertyCreateDto dto, Guid userId);
    Task<List<PropertyResponseDto>> GetProperties();
    Task<List<PropertyResponseDto>> GetPropertiesByHost(Guid userId);
    Task<PropertyResponseDto> UpdateProperty(PropertyResponseDto dto, Guid userId, Guid propertyId);
}