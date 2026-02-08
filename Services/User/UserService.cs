using Microsoft.AspNetCore.Identity;
using vizin.Repositories.User;
using vizin.DTO.User;
using vizin.Models;
using vizin.Services.User.Interface;

namespace vizin.Services.User;

public class UserService : IUserService
{
    private IUserRepository _repository;
       public UserService(IUserRepository repository)
    {
        _repository = repository;
    }

   public async Task<UserResponseDTO?> GetUser(Guid userId)
    {
        var user = await _repository.SelectUserById(userId);
        if (user == null) return null;
        var userDto = new UserResponseDTO()
        {
            Name = user.Name,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            Type = (int)user.Type
        };
        return userDto;
    }

    public async Task<TbUser?> LoginUser(string email, string providedPassword)
    { 
        var user = await _repository.HandleLogin(email);
        if (user == null) 
        { 
            throw new Exception("Email ou senha incorretos."); 
        }
        
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(providedPassword, user.Password);
        
        if (!isPasswordValid)
        {
           throw new Exception("A senha está incorreta!");
        }
        return user;
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
            CreatedAt = createUser.CreatedAt,
            Type = (int)createUser.Type
        };
    }
}