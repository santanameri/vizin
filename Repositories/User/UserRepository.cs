using Microsoft.EntityFrameworkCore;
using vizin.Models;
using vizin.DTO.User;

namespace vizin.Repositories.User;


public class UserRepository : IUserRepository
{
    private readonly PostgresContext _context;

    public UserRepository(PostgresContext context)
        {
            _context = context;
        }

    public List <TbUser> SelectAllUsers()
    {
        return _context.TbUsers.ToList();
    }


    public async Task<TbUser?> GetUserByEmailAsync(string email)
    {
        return await _context.TbUsers.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<TbUser> CreateUserAsync(TbUser user)
    {
   
    var newUser = new TbUser
    {
        Id = Guid.NewGuid(),
        Name = user.Name,
        Email = user.Email,
        Password = user.Password, 
        Type = (int)user.Type, 
        CreatedAt = DateTime.UtcNow
    };

    _context.TbUsers.Add(newUser);
    await _context.SaveChangesAsync();

    return newUser;
    }
}