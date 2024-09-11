namespace BookingService.API.Dto;

public record class UserDetailsDto
(
    string Id,
    string Username,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber
);