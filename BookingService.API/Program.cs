using BookingService.API.Data;
using BookingService.API.Endpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var userConnString = builder.Configuration.GetConnectionString("Booking");
// Register DbContexts with their respective connection strings
builder.Services.AddDbContext<BookingDbContext>(options =>
    options.UseSqlite(userConnString));
builder.Services.AddHttpClient();

// build app 
var app = builder.Build();
// Map endpoints
app.MapBookingEndpoints();
await app.MigrateDbAsync();
// run app
app.Run();