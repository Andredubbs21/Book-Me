using BookingService.API.Data;
using BookingService.API.Endpoints;
using Microsoft.EntityFrameworkCore;
using BookingService.API.Services;

var builder = WebApplication.CreateBuilder(args);
var userConnString = builder.Configuration.GetConnectionString("Booking");
// Register DbContexts with their respective connection strings
builder.Services.AddDbContext<BookingDbContext>(options =>
    options.UseSqlite(userConnString));
builder.Services.AddHttpClient();
builder.Services.AddControllers(); // Add this line for controllers

// Register RabbitMQConsumerService as a hosted service
builder.Services.AddHostedService<RabbitMQCancelConsumerService>();

// build app 
var app = builder.Build();
app.UseRouting();

// Map controllers
app.MapControllers(); // Replace endpoint mapping with controller mapping

await app.MigrateDbAsync();
// run app
app.Run();