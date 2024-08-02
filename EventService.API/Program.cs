using Microsoft.EntityFrameworkCore;
using EventService.API.Data;
using EventService.API.Endpoints;
using EventService.Api.Data;

var builder = WebApplication.CreateBuilder(args);
var userConnString = builder.Configuration.GetConnectionString("Event");
// Register DbContexts with their respective connection strings
builder.Services.AddDbContext<EventDbContext>(options =>
    options.UseSqlite(userConnString));
// build app 
var app = builder.Build();
// Map endpoints
app.MapEventEndpoints();
await app.MigrateDbAsync();
// run app
app.Run();