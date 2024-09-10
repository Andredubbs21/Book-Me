namespace NotificationService.Dto;

public record class CreateUserDto
(
    int Id,
    string Username,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber
);