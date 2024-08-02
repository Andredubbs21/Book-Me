using Microsoft.EntityFrameworkCore;
using UserService.API.Data;
using UserService.API.Endpoints;

var builder = WebApplication.CreateBuilder(args);
var userConnString = builder.Configuration.GetConnectionString("User");
// Register DbContexts with their respective connection strings
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlite(userConnString));

// build app 
var app = builder.Build();
// Map endpoints
app.MapUserEndpoints();
await app.MigrateDbAsync();
// run app
app.Run();