using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using NUnit.Framework;
using vizin.Models;
using vizin.Services.User;

namespace vizin.Tests.Services.User;

[TestFixture]
public class TokenServiceTest
{
    private TokenService _tokenService;

    [SetUp]
    public void SetUp()
    {
        _tokenService = new TokenService();
    }

    [Test]
    public void GenerateToken_ShouldGenerateValidToken_WithCorrectClaims()
    {
        var user = new TbUser 
        { 
            Id = Guid.NewGuid(), 
            Type = 1 // Anfitrião
        };
        
        var tokenString = _tokenService.GenerateToken(user);

        Assert.That(tokenString, Is.Not.Null.Or.Empty);

        // Decodificando o token para ler o que tem dentro
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(tokenString);

        // Validando o ID do usuário (NameIdentifier)
        var userIdClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
        Assert.That(userIdClaim, Is.EqualTo(user.Id.ToString()));

        // Validando a Role
        var roleClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
        Assert.That(roleClaim, Is.EqualTo("Anfitriao"));
    }

    [Test]
    public void GenerateToken_ShouldHaveExpiredDateCorrected()
    {
        var user = new TbUser { Id = Guid.NewGuid(), Type = 1 };
        
        var tokenString = _tokenService.GenerateToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(tokenString);
        
        Assert.That(jsonToken.ValidTo, Is.GreaterThan(DateTime.UtcNow));
        Assert.That(jsonToken.ValidTo, Is.LessThanOrEqualTo(DateTime.UtcNow.AddHours(2).AddMinutes(1)));
    }
}