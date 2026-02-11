using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using vizin;
using vizin.Models;
using vizin.Repositories.Property;
using vizin.Repositories.Property.Interfaces;
using vizin.Services.Property;
using vizin.Services.Property.Interfaces;
using vizin.Repositories.User;
using vizin.Services.User;
using vizin.Services.User.Interface;

var builder = WebApplication.CreateBuilder(args);
var key = Encoding.UTF8.GetBytes(Configuration.PrivateKey);
// Configura a Autenticação
builder.Services.AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false; // Defina como true em produção
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false, // Você pode colocar o nome da sua API aqui depois
            ValidateAudience = false
        };
    });


builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<PostgresContext>();

builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();
builder.Services.AddScoped<IPropertyService, PropertyService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
