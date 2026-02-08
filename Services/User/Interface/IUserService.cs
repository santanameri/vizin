using Microsoft.AspNetCore.Identity;
using vizin.DTO.User;
using vizin.Models;

namespace vizin.Services.User.Interface;

public interface IUserService
{
    public Task<UserResponseDTO?> GetUser(Guid id);
    public Task<UserResponseDTO> CreateUser(CreateUserRequestDTO request);
    public Task<TbUser?> LoginUser(string email, string providedPassword, PasswordHasher<TbUser> passwordHasher);
}