using Microsoft.EntityFrameworkCore;
using EventService.API.Data;

namespace EventService.Api.Data;
    public static class DataExtensions
    {
        public static async Task MigrateDbAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var eventContext = scope.ServiceProvider.GetRequiredService<EventDbContext>();
            await eventContext.Database.MigrateAsync();
        }
    }