using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
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


    public List<PropertyResponseDto> GetProperties()
    {
        var properties = _propertyRepository.SelectAllProperties();
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

    public async Task<PropertyResponseDto> CreateProperty(
    PropertyCreateDto dto,
    Guid userId
    )
    {
        //Console.WriteLine($"userId: {userId}");

        TbUser? user = await _userRepository.SelectUserById(userId);

       //Console.WriteLine($"user: {user}");

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
            UserId = userId
        };

        TbProperty created = _propertyRepository.Create(property);

        return new PropertyResponseDto
        {
            Id = created.Id,
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

    public async Task<PropertyResponseDto> UpdateDailyValueAsync(
        Guid propertyId,
        Guid userId,
        PropertyUpdateDailyValueDto dto
    )
    {
        //var property = await _propertyRepository.GetByIdAsync(propertyId);

        var property  = await _propertyRepository.GetPropertyById(propertyId);

        if (property == null)
            throw new KeyNotFoundException("Propriedade não encontrada");
        
        //Console.WriteLine($"property user id, {property.UserId}");
        //Console.WriteLine($"user id, {userId}");

        // 🔒 REGRA DE NEGÓCIO PRINCIPAL
        if (property.UserId != userId)
            throw new UnauthorizedAccessException("Você não é o proprietário deste imóvel.");

        if (dto.DailyValue <= 0)
            throw new ArgumentException("O valor da diária deve ser maior que zero.");

        property.DailyValue = dto.DailyValue!.Value;

        await _propertyRepository.UpdateAsync(property);

        return new PropertyResponseDto
        {
            Id = property.Id,
            Title = property.Title,
            Description = property.Description,
            FullAddress = property.FullAddress,
            Availability = property.Availability,
            DailyValue = property.DailyValue,
            Capacity = property.Capacity,
            AccomodationType = property.AccomodationType,
            PropertyCategory = property.PropertyCategory
        };
    }


}