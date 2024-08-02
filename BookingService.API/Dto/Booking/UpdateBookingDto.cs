namespace BookingService.API.Dto.booking;

public record class UpdateBookingDto
(
    string Username,
    int EventId,
    int Amount
);