using vizin.DTO.Property;
using vizin.DTO.Property.Amenity;
using vizin.Models;

namespace vizin.Services.Property;

public static class PropertyMapper
{
    public static PropertyResponseDto ToDto(this TbProperty property)
    {
        return new PropertyResponseDto()
        {
            Title = property.Title,
            Description = property.Description,
            FullAddress = property.FullAddress,
            Availability = property.Availability,
            DailyValue = (decimal)property.DailyValue,
            Capacity = property.Capacity,
            AccomodationType = property.AccomodationType,
            PropertyCategory = property.PropertyCategory,
            Amenities = property.Amenities.Select(a => new AmenityResponseDto()
            {
                Id = a.Id,
                Name = a.Name,
            }).ToList() ?? new List<AmenityResponseDto>()
        };
            
    }
}