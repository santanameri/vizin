using Microsoft.AspNetCore.Identity;
using vizin.Models;
using vizin.DTO.User;

namespace vizin.Repositories.User
{
    public interface IUserRepository
    {
       public Task<TbUser?> SelectUserById(Guid id);
       public Task<TbUser?> HandleLogin(string email);
       public Task<TbUser?> GetUserByEmailAsync(string email);
       public Task<TbUser> CreateUserAsync(TbUser user);
    }
}