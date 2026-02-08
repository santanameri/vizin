using vizin.Models;
using vizin.Repositories.User;
using vizin.DTO.User;

public interface IUserService
{
    public List<UserResponseDTO> GetUsers();

    public Task<UserResponseDTO> CreateUser(CreateUserRequestDTO request);
       
}