using Microsoft.EntityFrameworkCore;

namespace BookingService.API.Data;

 public static class DataExtensions
    {
        public static async Task MigrateDbAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var bookingContext = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
            await bookingContext.Database.MigrateAsync();
        }
    }