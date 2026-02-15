using vizin.DTO.Property;
using vizin.DTO.Property.Amenity;

namespace vizin.Services.Property.Interfaces;

public interface IPropertyService
{
    Task<PropertyResponseDto> CreateProperty(PropertyCreateDto dto, Guid userId);
    Task<List<PropertyResponseDto>> GetProperties();
    Task<List<PropertyResponseDto>> GetPropertiesByHost(Guid userId);
    Task<PropertyResponseDto> UpdateProperty(PropertyResponseDto dto, Guid userId, Guid propertyId);
    Task<PropertyResponseDto> UpdateDailyValueAsync(Guid propertyId, Guid userId, PropertyUpdateDailyValueDto dto);
    Task<PropertyResponseDto> AddAmenitiesAsync(Guid amenityId, Guid propertyId);
    Task<List<AmenityResponseDto>> GetAllAmenities();
}