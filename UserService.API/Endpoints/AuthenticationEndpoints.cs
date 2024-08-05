using Microsoft.AspNetCore.Identity;
using UserService.API.Dto;
using UserService.API.Entities;
namespace UserService.API.Endpoints;

public static class AuthenticationEndpoints
{
public static RouteGroupBuilder MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("auth").WithParameterValidation();

        // Login endpoint
        group.MapPost("/login", async (LoginDto loginDto, UserManager<UserIdentity> userManager, SignInManager<UserIdentity> signInManager
        //,ITokenService tokenService
         ) =>
        {
            // Find the user by username
            var user = await userManager.FindByNameAsync(loginDto.Username);

            if (user is null)
            {
                return Results.Unauthorized();
            }

            // Check the password
            var result = await signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!result.Succeeded)
            {
                return Results.Unauthorized();
            }

            // Generate a JWT or another type of token
        //    var token = tokenService.GenerateToken(user);

            return Results.Ok(
            //new
           // {
             //   Token = token,
               // UserId = user.Id,
                //Username = user.UserName,
                //FirstName = user.FirstName,
                //LastName = user.LastName,
                //Email = user.Email
            //}
            );
        });

        // Login by params
        group.MapPost("/login_params/{username}/{password}", async(string username, string password,
        UserManager<UserIdentity> userManager, SignInManager<UserIdentity> signInManager) => {
          // Find the user by username
            var user = await userManager.FindByNameAsync(username);

            if (user is null)
            {
                return Results.Unauthorized();
            }

            // Check the password
            var result = await signInManager.CheckPasswordSignInAsync(user,password, false);

            if (!result.Succeeded)
            {
                return Results.Unauthorized();
            }

            // Generate a JWT or another type of token
        //    var token = tokenService.GenerateToken(user);

            return Results.Ok(
            //new
           // {
             //   Token = token,
               // UserId = user.Id,
                //Username = user.UserName,
                //FirstName = user.FirstName,
                //LastName = user.LastName,
                //Email = user.Email
            //}
            );            
        });
        return group;
    }
}
