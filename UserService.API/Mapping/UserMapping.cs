using UserService.API.Dto;
using UserService.API.Entities;
namespace UserService.Api.Mapping;


public static class UserMapping
{
    // Maps a create user data transfer object to a database entity 
    public static UserIdentity ToEntity(this CreateUserDto user){
        return new UserIdentity(){
            UserName = user.UserName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber
        };
    }

    // Maps a update user data transfer object to a database entity 
    public static UserIdentity ToEntity(this UpdateUserDto user, string id){
        return new UserIdentity(){
            Id = id,
            UserName = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber
        };
    }

    //Maps an entity to a summarized user data transfer object
    public static UserSummaryDto ToUserSummaryDto(this UserIdentity user){
        return new(
            user.UserName!,
            user.LastName,
            user.Email!
        );
    }
     //Maps an entity to a detailed user data transfer object
    public static UserDetailsDto ToUserDetailsDto(this UserIdentity user){
        return new(
            user.Id,
            user.UserName!,
            user.FirstName!,
            user.LastName,
            user.Email!,
            user.PhoneNumber ?? string.Empty
        );
    }
}