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

}