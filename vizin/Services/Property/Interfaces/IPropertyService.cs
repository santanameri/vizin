using vizin.DTO.Booking;
using vizin.DTO.Property;
using vizin.DTO.Property.Amenity;

namespace vizin.Services.Property.Interfaces;

public interface IPropertyService
{
    Task<PropertyResponseDto> CreateProperty(PropertyCreateDto dto, Guid userId);
    Task<List<PropertyResponseDto>> GetProperties();
    Task<List<PropertyResponseDto>> GetPropertiesByHost(Guid hostId);
    Task<PropertyResponseDto> UpdateProperty(PropertyCreateDto dto, Guid userId, Guid propertyId);
    Task<PropertyResponseDto> UpdateDailyValueAsync(Guid propertyId, Guid userId, PropertyUpdateDailyValueDto dto);
    Task<PropertyResponseDto> AddAmenitiesAsync(Guid amenityId, Guid propertyId, Guid userId);
    Task<List<AmenityResponseDto>> GetAllAmenities();
    Task<List<PropertyResponseDto>> FilterProperties(PropertyFilterParams filters);
    Task<PropertyResponseDto> RemoveAmenityAsync(Guid amenityId, Guid propertyId, Guid userId);
    Task<List<PropertyResponseDto>> FilterByAmenitiesAsync(List<Guid> amenityIds, bool matchAll);
    Task<List<PropertyResponseDto>> GetAvailableProperties(AvailabilityFilterDto filter);

}