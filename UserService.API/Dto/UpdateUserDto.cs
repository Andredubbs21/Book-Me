namespace UserService.API.Dto;

public record class UpdateUserDto
(
    string Username,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string Password,
    string CurrentPassword
);