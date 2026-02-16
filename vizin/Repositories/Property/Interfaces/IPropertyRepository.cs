using vizin.DTO.Property;
using vizin.Models;

namespace vizin.Repositories.Property.Interfaces;

public interface IPropertyRepository
{
    Task<TbProperty> Create(TbProperty property);
    Task<List<TbProperty>> SelectAllProperties();
    Task<List<TbProperty>> SelectAllPropertiesByHost(Guid hostId);
    Task<TbProperty?> GetPropertyById(Guid propertyId);
    Task<TbProperty?> Update(Guid propertyId, TbProperty property);
    Task PatchAsync(TbProperty property);
    Task<List<TbAmenity>> SelectAllAmenity();
    Task<TbAmenity?> GetAmenityById(Guid amenityId);
    Task<TbProperty?> AddAmenityAsync(Guid amenityId, Guid propertyId);
}