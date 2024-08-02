using UserService.API.Dto;
using UserService.API.Entities;
namespace UserService.Api.Mapping;


public static class UserMapping
{
    // Maps a create user data transfer object to a database entity 
    public static User ToEntity(this CreateUserDto user){
        return new User(){
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber
        };
    }

    // Maps a update user data transfer object to a database entity 
    public static User ToEntity(this UpdateUserDto user, int id){
        return new User(){
            Id = id,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber
        };
    }

    //Maps an entity to a summarized user data transfer object
    public static UserSummaryDto ToUserSummaryDto(this User user){
        return new(
            user.Username,
            user.LastName,
            user.Email
        );
    }
     //Maps an entity to a detailed user data transfer object
    public static UserDetailsDto ToUserDetailsDto(this User user){
        return new(
            user.Id,
            user.Username,
            user.FirstName,
            user.LastName,
            user.Email,
            user.PhoneNumber ?? string.Empty
        );
    }
}