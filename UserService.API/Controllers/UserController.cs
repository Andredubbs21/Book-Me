using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserService.API.Data;
using UserService.API.Dto;
using UserService.API.Entities;
using UserService.Api.Mapping;

namespace UserService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserIdentityDbContext _dbContext;
        private readonly UserManager<UserIdentity> _userManager;

        public UserController(UserIdentityDbContext dbContext, UserManager<UserIdentity> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDetailsDto>>> GetAllUsers()
        {
            var users = await _dbContext.Users
                .Select(user => user.ToUserDetailsDto())
                .AsNoTracking()
                .ToListAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDetailsDto>> GetUserById(string id)
        {
            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            return Ok(user.ToUserDetailsDto());
        }

        [HttpGet("user_name/{username}")]
        public async Task<ActionResult<UserDetailsDto>> GetUserByUsername(string username)
        {
            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null)
                return NotFound();

            return Ok(user.ToUserDetailsDto());
        }

        [HttpPost]
        public async Task<ActionResult<UserDetailsDto>> CreateUser(CreateUserDto newUser)
        {
            var user = newUser.ToEntity();
            var result = await _userManager.CreateAsync(user, newUser.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user.ToUserDetailsDto());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, UpdateUserDto updatedUser)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            user.UserName = updatedUser.Username;
            user.FirstName = updatedUser.FirstName;
            user.LastName = updatedUser.LastName;
            user.Email = updatedUser.Email;
            user.PhoneNumber = updatedUser.PhoneNumber;

            if (!string.IsNullOrEmpty(updatedUser.Password))
            {
                var passwordResult = await _userManager.ChangePasswordAsync(user, updatedUser.CurrentPassword, updatedUser.Password);
                if (!passwordResult.Succeeded)
                    return BadRequest(passwordResult.Errors);
            }

            await _userManager.UpdateAsync(user);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _dbContext.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("user_name/{username}")]
        public async Task<IActionResult> DeleteUserByUsername(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return NotFound(new { Message = "User not found" });

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(new { Message = "Failed to delete user", Errors = result.Errors });

            return NoContent();
        }
    }
}
