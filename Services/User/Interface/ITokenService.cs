using System.Security.Claims;
using vizin.Models;

namespace vizin.Services.User.Interface;

public interface ITokenService
{
    public string GenerateToken(TbUser user);
}