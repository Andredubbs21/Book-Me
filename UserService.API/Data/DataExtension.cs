﻿using Microsoft.EntityFrameworkCore;
namespace UserService.API.Data;

    public static class DataExtensions
    {
        public static async Task MigrateDbAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var userContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
            await userContext.Database.MigrateAsync();
        }
    }
