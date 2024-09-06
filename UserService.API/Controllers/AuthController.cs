using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserService.API.Dto;
using UserService.API.Entities;

namespace UserService.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<UserIdentity> _userManager;
        private readonly SignInManager<UserIdentity> _signInManager;
      //  private readonly ITokenService _tokenService;

        public AuthController(UserManager<UserIdentity> userManager, SignInManager<UserIdentity> signInManager /*, ITokenService tokenService*/)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            //_tokenService = tokenService;
        }

        // Login endpoint
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            // Find the user by username
            var user = await _userManager.FindByNameAsync(loginDto.Username);
            if (user is null)
            {
                return Unauthorized();
            }

            // Check the password
            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded)
            {
                return Unauthorized();
            }

            // Generate a JWT or another type of token
            // var token = _tokenService.GenerateToken(user);

            return Ok(
            // new
            // {
            //     // Token = token,
            //  //   UserId = user.Id,
            //   //  Username = user.UserName,
            //    // FirstName = user.FirstName,
            //    // LastName = user.LastName,
            //    // Email = user.Email
            // }
            );
        }

        // Login by params endpoint
        [HttpPost("login_params/{username}/{password}")]
        public async Task<IActionResult> LoginWithParams(string username, string password)
        {
            // Find the user by username
            var user = await _userManager.FindByNameAsync(username);
            if (user is null)
            {
                return Unauthorized();
            }

            // Check the password
            var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
            if (!result.Succeeded)
            {
                return Unauthorized();
            }

            //Generate a JWT or another type of token
            //var token = _tokenService.GenerateToken(user);

            return Ok(
            //    new
            // {
            //   Token = token,
            //   UserId = user.Id,
            //   Username = user.UserName,
            //   FirstName = user.FirstName,
            //   LastName = user.LastName,
            // Email = user.Email
            // }
            );
        }
    }
}
