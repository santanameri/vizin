using Microsoft.EntityFrameworkCore;
using vizin.Models;

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
}