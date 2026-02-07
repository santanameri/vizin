using vizin.Models;
<<<<<<< Updated upstream
using vizin.Repositories.Property;
using vizin.Repositories.Property.Interfaces;
using vizin.Services.Property;
using vizin.Services.Property.Interfaces;
=======
using vizin.Repositories.User;
using vizin.Services.User;
using vizin.Services;

>>>>>>> Stashed changes

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<PostgresContext>();

<<<<<<< Updated upstream
builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();
builder.Services.AddScoped<IPropertyService, PropertyService>();
=======
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
>>>>>>> Stashed changes

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
