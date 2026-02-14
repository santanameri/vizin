using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Http.HttpResults;
using vizin.Repositories.User;
using vizin.DTO.Property;
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
        var response = new List<PropertyResponseDto>();

        foreach (var property in properties)
        {
            response.Add(new PropertyResponseDto
            {
                Title = property.Title,
                Description = property.Description,
                FullAddress = property.FullAddress,
                Availability = property.Availability,
                DailyValue = (decimal)property.DailyValue,
                Capacity = property.Capacity,
                AccomodationType = property.AccomodationType,
                PropertyCategory = property.PropertyCategory
            });
        }

        return response;
    }
    
    public async Task<List<PropertyResponseDto>> GetPropertiesByHost(Guid userId)
    {
        TbUser? user = await _userRepository.SelectUserById(userId);

        if (user == null)
            throw new Exception("Usuário não encontrado");
        
        var properties = await _propertyRepository.SelectAllPropertiesByHost(userId);
        var response = new List<PropertyResponseDto>();

        foreach (var property in properties)
        {
            response.Add(new PropertyResponseDto
            {
                Title = property.Title,
                Description = property.Description,
                FullAddress = property.FullAddress,
                Availability = property.Availability,
                DailyValue = (decimal)property.DailyValue,
                Capacity = property.Capacity,
                AccomodationType = property.AccomodationType,
                PropertyCategory = property.PropertyCategory
            });
        }

        return response;
    }

    public async Task<PropertyResponseDto> CreateProperty(PropertyCreateDto dto, Guid userId)
    {
        TbUser? user = await _userRepository.SelectUserById(userId);

        if (user == null)
            throw new Exception("Usuário não encontrado");
        
        if (user.Type != 1)
            throw new Exception("Apenas usuários do tipo Anfitrião podem cadastrar imóveis");
        
        TbProperty property = new TbProperty
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            FullAddress = dto.FullAddress,
            Availability = dto.Availability,
            DailyValue = dto.DailyValue!.Value,
            Capacity = dto.Capacity!.Value,
            AccomodationType = dto.AccomodationType!.Value,
            PropertyCategory = dto.PropertyCategory!.Value,
            UserId = user.Id
        };

        TbProperty created = _propertyRepository.Create(property);

        return new PropertyResponseDto
        {
            Title = created.Title,
            Description = created.Description,
            FullAddress = created.FullAddress,
            Availability = created.Availability, 
            DailyValue = (decimal)created.DailyValue,
            Capacity = created.Capacity,
            AccomodationType = created.AccomodationType,
            PropertyCategory = created.PropertyCategory
        };
    }

    public async Task<PropertyResponseDto> UpdateProperty(PropertyResponseDto dto, Guid userId, Guid propertyId)
    {
        var property  = await _propertyRepository.GetPropertyById(propertyId);
        Console.WriteLine(property);
        if (property == null)
        {
            throw new KeyNotFoundException("Imóvel não encontrado.");
        }
        
        var user = await _userRepository.SelectUserById(userId);

        if (user == null)
        {
            throw new KeyNotFoundException("Usuário não encontrado.");
        }
        
        if (property.UserId != userId)
        {
            throw new UnauthorizedAccessException("Você não ten permissão para editar este imóvel.");
        }

        if (dto.DailyValue <= 0)
        {
            throw new ArgumentException("O valor da diária deve ser maior que 0.");
        }
        
        property.Title = dto.Title;
        property.Description = dto.Description;
        property.FullAddress = dto.FullAddress;
        property.Availability = dto.Availability;
        property.DailyValue = dto.DailyValue;
        property.Capacity = dto.Capacity;
        property.AccomodationType = dto.AccomodationType;
        property.PropertyCategory = dto.PropertyCategory;
        
        var result = await _propertyRepository.Update(propertyId, property);

        return new PropertyResponseDto
        {
            Title = result.Title,
            Description = result.Description,
            FullAddress = result.FullAddress,
            Availability = result.Availability,
            DailyValue = result.DailyValue,
            Capacity = result.Capacity,
            AccomodationType = result.AccomodationType,
            PropertyCategory = result.PropertyCategory
        };
    }
}