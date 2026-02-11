using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using vizin.Models;
using vizin.Models.Enum;
using vizin.Services.User.Interface;

namespace vizin.Services.User;

public class TokenService : ITokenService
{
    public string GenerateToken(TbUser user)
    { 
        // criação do token
        var handler = new JwtSecurityTokenHandler();

        var key = Encoding.UTF8.GetBytes(Configuration.PrivateKey);
        var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = GenerateClaims(user),
            SigningCredentials = credentials,
            Expires = DateTime.UtcNow.AddHours(2),
        };
        var token = handler.CreateToken(tokenDescriptor);
     
        var stringToken = handler.WriteToken(token);
        return stringToken;
    }

    private static ClaimsIdentity GenerateClaims(TbUser user)
    {
        // payload do token
        var ci = new ClaimsIdentity();
        
        // vai permitir que pegue o usuário logado - User.Identity.Id 
        ci.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
        
        // roles - User.IsInRole + User.Authorize 
        ci.AddClaim(new Claim(ClaimTypes.Role, ((UserType)user.Type).ToString()));
        return ci;
    }
}