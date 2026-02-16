using vizin.Repositories.User;
using vizin.DTO.Property;
using vizin.DTO.Property.Amenity;
using vizin.Models;
using vizin.Repositories.Property.Interfaces;
using vizin.Services.Property.Interfaces;

namespace vizin.Services.Property;

public class PropertyService : IPropertyService
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IUserRepository _userRepository;


    public PropertyService(
        IPropertyRepository propertyRepository,
        IUserRepository userRepository
    )
    {
        _propertyRepository = propertyRepository;
        _userRepository = userRepository;
    }
    
    public async Task<List<PropertyResponseDto>> GetProperties()
    {
        var properties = await _propertyRepository.SelectAllProperties();

        return properties.Select(property => property.ToDto()).ToList();
    }
    
    public async Task<List<PropertyResponseDto>> GetPropertiesByHost(Guid hostId)
    {
        var properties = await _propertyRepository.SelectAllPropertiesByHost(hostId);

        return properties.Select(property => property.ToDto()).ToList();
    }

    public async Task<PropertyResponseDto> CreateProperty(PropertyCreateDto dto, Guid userId)
    {
        TbUser? user = await _userRepository.SelectUserById(userId);

        if (user == null)
            throw new KeyNotFoundException("Usuário não encontrado");
        
        if (dto.DailyValue is null or <= 0)
            throw new ArgumentException("Diária inválida");

        if (dto.Capacity is null or <= 0)
            throw new ArgumentException("Capacidade inválida");

        if (dto.AccomodationType is null or <= 0)
            throw new ArgumentException("Tipo de acomodação inválida.");

        if (dto.PropertyCategory is null or <= 0)
            throw new ArgumentException("Tipo de categoria errada");

        TbProperty property = new TbProperty
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            FullAddress = dto.FullAddress,
            Availability = dto.Availability,
            DailyValue = dto.DailyValue.Value,
            Capacity = dto.Capacity.Value,
            AccomodationType = dto.AccomodationType.Value,
            PropertyCategory = dto.PropertyCategory.Value,
            UserId = user.Id
        };

        TbProperty created = await _propertyRepository.Create(property);

       return  created.ToDto();
    }

    public async Task<PropertyResponseDto> UpdateProperty(PropertyCreateDto dto, Guid userId, Guid propertyId)
    {
        var property  = await _propertyRepository.GetPropertyById(propertyId);
       
        if (property == null)
            throw new KeyNotFoundException("Imóvel não encontrado.");

        if (property.UserId != userId)
            throw new UnauthorizedAccessException("Você não ten permissão para editar este imóvel.");
        
        if (dto.DailyValue is null or <= 0)
            throw new ArgumentException("Diária inválida");

        if (dto.Capacity is null or <= 0)
            throw new ArgumentException("Capacidade inválida");

        if (dto.AccomodationType is null or <= 0)
            throw new ArgumentException("Tipo de acomodação inválida.");

        if (dto.PropertyCategory is null or <= 0)
            throw new ArgumentException("Tipo de categoria errada");
        
        property.Title = dto.Title;
        property.Description = dto.Description;
        property.FullAddress = dto.FullAddress;
        property.Availability = dto.Availability;
        property.DailyValue = dto.DailyValue.Value;
        property.Capacity = dto.Capacity.Value;
        property.AccomodationType = dto.AccomodationType.Value;
        property.PropertyCategory = dto.PropertyCategory.Value;
        
        var result = await _propertyRepository.Update(propertyId, property);

        return result.ToDto();
    }
    
    public async Task<PropertyResponseDto> UpdateDailyValueAsync(Guid propertyId, Guid userId, PropertyUpdateDailyValueDto dto)
    {
        var property  = await _propertyRepository.GetPropertyById(propertyId);

        if (property == null)
            throw new KeyNotFoundException("Propriedade não encontrada");
        
        if (property.UserId != userId)
            throw new UnauthorizedAccessException("Você não é o proprietário deste imóvel.");

        if (dto.DailyValue is null or <= 0)
            throw new ArgumentException("O valor da diária deve ser maior que zero.");

        property.DailyValue = dto.DailyValue.Value;

        await _propertyRepository.PatchAsync(property);

        return property.ToDto();
    }

    public async Task<PropertyResponseDto> AddAmenitiesAsync(Guid amenityId, Guid propertyId, Guid userId)
    { 
      var property = await _propertyRepository.GetPropertyById(propertyId);
      
      if (property == null)
          throw new KeyNotFoundException("Imóvel não encontrado");
      
      if (property.UserId != userId)
          throw new UnauthorizedAccessException("Você não tem permissão para editar este imóvel.");
      
      var amenity = await _propertyRepository.GetAmenityById(amenityId);
      
      var updatedProperty = await _propertyRepository.AddAmenityAsync(amenityId, propertyId);

      if (updatedProperty == null)
          throw new KeyNotFoundException("Comodidade não encontrada");
      
      return property.ToDto();
    }

    public async Task<List<AmenityResponseDto>> GetAllAmenities()
    {
        var result = await _propertyRepository.SelectAllAmenity();
        return result.Select(a => new AmenityResponseDto
        {
            Id = a.Id,
            Name = a.Name,
        }).ToList();
    }
}