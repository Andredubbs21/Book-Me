using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserService.API.Entities;

namespace UserService.API.Data;

public class UserIdentityDbContext(DbContextOptions<UserIdentityDbContext> options) : IdentityDbContext<UserIdentity>(options)
{
}
