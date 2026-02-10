using vizin.Repositories;
using vizin.Repositories.User;
using vizin.DTO.User;
using vizin.Models;

namespace vizin.Services.User;

public class UserService : IUserService
{
    private IUserRepository _repository;
       public UserService(IUserRepository repository)
    {
        _repository = repository;
    }

   public List<UserResponseDTO> GetUsers()
    {
        
         List<TbUser> tbUsers = _repository.SelectAllUsers();

        List<UserResponseDTO> usersDTO = new List<UserResponseDTO>();

        foreach(TbUser tbUser in tbUsers)
        {
            UserResponseDTO usuarioRetorno = new UserResponseDTO();
            usuarioRetorno.Id = tbUser.Id;
            usuarioRetorno.Name = tbUser.Name;
            usuarioRetorno.Email = tbUser.Email;
            usuarioRetorno.CreatedAt = tbUser.CreatedAt;

            usersDTO.Add(usuarioRetorno);
        }

        return usersDTO;
    }

    public async Task<UserResponseDTO> CreateUser(CreateUserRequestDTO request)
    {
        var existingUser = await _repository.GetUserByEmailAsync(request.Email);

        if(existingUser != null)
        {
            throw new InvalidOperationException($"Email '{request.Email}' já está cadastrado!");
        }

        var newUser = new TbUser
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Type = (int)request.Type,
            CreatedAt = DateTime.UtcNow
        };

        var createUser = await _repository.CreateUserAsync(newUser);

        return new UserResponseDTO
        {
            Id = createUser.Id,
            Name = createUser.Name,
            Email = createUser.Email,
            CreatedAt = createUser.CreatedAt
        };
    }

}