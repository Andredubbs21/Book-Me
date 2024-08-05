using Microsoft.AspNetCore.Identity;

namespace UserService.API.Entities;

public class UserIdentity: IdentityUser
{
    public  string? FirstName {get; set;}
    public required string LastName {get; set;}

}