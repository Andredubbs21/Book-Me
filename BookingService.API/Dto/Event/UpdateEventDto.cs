namespace BookingService.API.Dto;


public record class UpdateEventDto
(
    string Name,
    string Description,
    DateTime Date,
    int MaxCapacity
);