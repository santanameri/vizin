using vizin.DTO.Property;

namespace vizin.Services.Property.Interfaces;

public interface IPropertyService
{
    Task<PropertyResponseDto> CreateProperty(
        PropertyCreateDto dto,
        Guid userId
    );

    List<PropertyResponseDto> GetProperties();
}