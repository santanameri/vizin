using vizin.Models;

namespace vizin.Repositories.User
{
    public interface IUserRepository
    {
       public List <TbUser> SelectAllUsers();
    }
}