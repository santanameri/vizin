using vizin.DTO.Property;
using vizin.Models;
using vizin.Repositories.Property.Interfaces;
using vizin.Services.Property.Interfaces;

namespace vizin.Services.Property;

public class PropertyService : IPropertyService
{
    private IPropertyRepository _repository;

    public PropertyService(IPropertyRepository repository)
    {
        _repository = repository;
    }

    public List<PropertyResponseDto> GetProperties()
    {
        List<TbProperty> properties = _repository.SelectAllProperties();
        List<PropertyResponseDto> response = new List<PropertyResponseDto>();
        foreach (TbProperty property in properties)
        {
            PropertyResponseDto responseDto = new PropertyResponseDto();
            responseDto.Titulo = property.Title;
            responseDto.Description = property.Description;
            responseDto.FullAddress = property.FullAddress;
            responseDto.IsAvailable = property.Availability;
            responseDto.Capacity = property.Capacity;
            responseDto.Diaria = (float)property.DailyValue;
            responseDto.TipoDeAcomodacao = property.AccomodationType;
            responseDto.Categoria = property.PropertyCategory;
            
            response.Add(responseDto);
        }
        return response;
    } 
    
}