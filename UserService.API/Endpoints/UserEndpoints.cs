using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserService.Api.Mapping;
using UserService.API.Data;
using UserService.API.Dto;
using UserService.API.Entities;

namespace UserService.API.Endpoints;
public static class UserEndpoints
{
    public static RouteGroupBuilder MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("users").WithParameterValidation();
        const string getUserEndpoint = "GetUserById";

        // Get all users
        group.MapGet("/", async (UserIdentityDbContext dbContext) =>
            await dbContext.Users
                .Select(user => user.ToUserDetailsDto())
                .AsNoTracking()
                .ToListAsync());

        // Get user by id
        group.MapGet("/{id}", async (string id, UserIdentityDbContext dbContext) =>
        {
            var user = await dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            return user is null ? Results.NotFound() : Results.Ok(user.ToUserDetailsDto());
        }).WithName(getUserEndpoint);

        // Get user by username
        group.MapGet("/user_name/{username}", async (string username, UserIdentityDbContext dbContext) =>
        {
            var user = await dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserName == username);

            return user is null ? Results.NotFound() : Results.Ok(user.ToUserDetailsDto());
        }).WithName("GetUserByUsername");

         // Create user with password
        group.MapPost("/", async (CreateUserDto newUser, UserManager<UserIdentity> userManager) =>
        {
            var user = newUser.ToEntity();

            var result = await userManager.CreateAsync(user, newUser.Password);

            if (!result.Succeeded)
            {
                return Results.BadRequest(result.Errors);
            }
            return Results.CreatedAtRoute(getUserEndpoint, new { id = user.Id }, user);
        });

        // Update user, including password
        group.MapPut("/{id}", async (string id, UpdateUserDto updatedUser, UserManager<UserIdentity> userManager) =>
        {
            var user = await userManager.FindByIdAsync(id);
            if (user is null)
            {
                return Results.NotFound();
            }

            user.UserName = updatedUser.Username;
            user.FirstName = updatedUser.FirstName;
            user.LastName = updatedUser.LastName;
            user.Email = updatedUser.Email;
            user.PhoneNumber = updatedUser.PhoneNumber;

            // If the password is being updated, update it securely
            if (!string.IsNullOrEmpty(updatedUser.Password))
            {
                var passwordResult = await userManager.ChangePasswordAsync(user, updatedUser.CurrentPassword, updatedUser.Password);
                if (!passwordResult.Succeeded)
                {
                    return Results.BadRequest(passwordResult.Errors);
                }
            }

            await userManager.UpdateAsync(user);

            return Results.NoContent();
        });


        // Delete user by id
        group.MapDelete("/{id}", async (string id, UserIdentityDbContext dbContext) =>
        {
            var user = await dbContext.Users.FindAsync(id);
            if (user is null)
            {
                return Results.NotFound();
            }

            dbContext.Users.Remove(user);
            await dbContext.SaveChangesAsync();

            return Results.NoContent();
        });

         // Delete user by username
        group.MapDelete("/user_name/{username}", async (string username, UserManager<UserIdentity> userManager) =>
        {
            // Find the user by username
            var user = await userManager.FindByNameAsync(username);

            if (user is null)
            {
                return Results.NotFound(new { Message = "User not found" });
            }

            // Delete the user
            var result = await userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                return Results.BadRequest(new { Message = "Failed to delete user", Errors = result.Errors });
            }

            return Results.NoContent();
        });

      return group;
    }
}
