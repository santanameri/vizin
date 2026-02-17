using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using vizin;
using vizin.Models;
using vizin.Repositories.Booking;
using vizin.Repositories.Booking.Interfaces;
using vizin.Repositories.Property;
using vizin.Repositories.Property.Interfaces;
using vizin.Services.Property;
using vizin.Services.Property.Interfaces;
using vizin.Repositories.User;
using vizin.Services.Booking;
using vizin.Services.Booking.Interfaces;
using vizin.Services.User;
using vizin.Services.User.Interface;
using vizin.Services.Favorite;           
using vizin.Services.Favorite.Interfaces; 
using vizin.Repositories;  


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
        
        x.Events = new JwtBearerEvents
        {
            OnForbidden = context =>
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json; charset=utf-8";
                return context.Response.WriteAsync(
                    "{\"error\":\"forbidden\",\"message\":\"Você não tem permissão para acessar este recurso.\"}",
                    Encoding.UTF8);
            },
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json; charset=utf-8";

                return context.Response.WriteAsync(
                    "{\"error\":\"unauthorized\",\"message\":\"Você precisa estar logado.\"}",
                    Encoding.UTF8
                );
            }
        };
    });

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Faz com que os Enums apareçam como Strings no JSON
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddOpenApi();

builder.Services.AddDbContext<PostgresContext>();

builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();
builder.Services.AddScoped<IPropertyService, PropertyService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddAuthorization((options =>
{
    options.AddPolicy("HospedeOnly", policy => policy.RequireRole("Hospede"));
    options.AddPolicy("AnfitriaoOnly", policy => policy.RequireRole("Anfitriao"));
}));

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
