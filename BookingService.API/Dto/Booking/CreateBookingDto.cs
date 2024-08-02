using System.ComponentModel.DataAnnotations;
namespace BookingService.API.Dto.booking;

public record class CreateBookingDto
(
    [Required][StringLength(50)]string Username,
    [Required] int EventId,
    [Required] int Amount
);