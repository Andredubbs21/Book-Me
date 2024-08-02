using UserService.Api.Mapping;
using UserService.API.Data;
using UserService.API.Dto;
using UserService.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace UserService.API.Endpoints;


public static class UserEndpoints
{
    public static RouteGroupBuilder MapUserEndpoints(this WebApplication app)
    {
        // Define a group route for the endpoints.
        var group = app.MapGroup("users").WithParameterValidation();
        const string getUserEndpoint = "GetUserById"; 

        // Get Users
        group.MapGet("/", async (UserDbContext dbContext) =>
            await dbContext.Users
                // Transforms users to summarized dto versions
                .Select(user => user.ToUserDetailsDto())
                // Removes tracking for better performance (optional)
                .AsNoTracking()
                // Makes list out of the query 
                .ToListAsync());

        // Get users by id
        group.MapGet("/{id}", async (int id, UserDbContext dbContext) =>
        {
            // Search for user by id. result may be null 
            User? user = await dbContext.Users.FindAsync(id);
            // Return either null or a detailed version of the user
            return user is null ? Results.NotFound() : Results.Ok(user.ToUserDetailsDto());
        }).WithName(getUserEndpoint);

        // Get users by username 
        group.MapGet("/user_name/{username}", async (string username, UserDbContext dbContext) =>
        {
            // Search for user by username. result may be null
            User? user = await dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == username);
            // Return either null or a detailed version of the user
            return user is null ? Results.NotFound() : Results.Ok(user.ToUserDetailsDto());
        }).WithName("GetUserByUsername");

        // Create Users
        group.MapPost("/", async (CreateUserDto newUser, UserDbContext dbContext) => 
        { 
            // Convert dto to entity 
            User user = newUser.ToEntity();
            // Add user
            dbContext.Users.Add(user);
            // Commit changes
            await dbContext.SaveChangesAsync();
            return Results.CreatedAtRoute(getUserEndpoint, new { id = user.Id }, user.ToUserDetailsDto());
        });

        // Update users by id 
        group.MapPut("/{id}", async(int id, UpdateUserDto updatedUser, UserDbContext dbContext) =>{
            var existingUser = await dbContext.Users.FindAsync(id);

            if (existingUser is null){
                return Results.NotFound();
            }
            dbContext.Entry(existingUser)
            .CurrentValues
            .SetValues(updatedUser.ToEntity(id));
            await dbContext.SaveChangesAsync();
            return Results.NoContent();
        });

        //  Delete users by id
        group.MapDelete("/{id}", async(int id, UserDbContext dbContext) =>{
            await dbContext.Users.Where(user => user.Id == id).ExecuteDeleteAsync();
            return Results.NoContent();
        });

        // Delete users by username
        group.MapDelete("/user_name/{username}", async (string username, UserDbContext dbContext) =>
        {
            // Search for user by username
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return Results.NotFound();
            }

            // Delete the user
            dbContext.Users.Remove(user);
            await dbContext.SaveChangesAsync();

            return Results.NoContent();
        });

        return group;
    }
}