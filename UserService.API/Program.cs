using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserService.API.Data;
using UserService.API.Endpoints;
using UserService.API.Entities;

var builder = WebApplication.CreateBuilder(args);
var userConnString = builder.Configuration.GetConnectionString("User");
// Register DbContexts with their respective connection strings
//builder.Services.AddDbContext<UserDbContext>(options =>
  //  options.UseSqlite(userConnString));
//Authentication
builder.Services.AddAuthentication()
.AddBearerToken(IdentityConstants.BearerScheme);

//Authorization
builder.Services.AddAuthorizationBuilder();
//Set database
builder.Services.AddDbContext<UserIdentityDbContext>(options =>
    options.UseSqlite(userConnString));

builder.Services.AddIdentityCore<UserIdentity>()
.AddEntityFrameworkStores<UserIdentityDbContext>()
.AddApiEndpoints();

builder.Services.AddControllers(); // Add this line for controllers

// build app 
var app = builder.Build();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapControllers(); // Replace endpoint mapping with controller mapping

// run app
await app.MigrateDbAsync();
app.Run();