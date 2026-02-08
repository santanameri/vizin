using vizin.Models;
using vizin.DTO.User;

namespace vizin.Repositories.User
{
    public interface IUserRepository
    {
       public List <TbUser> SelectAllUsers();

       public Task<TbUser?> GetUserByEmailAsync(string email);

       public Task<TbUser> CreateUserAsync(TbUser user);

    }
}