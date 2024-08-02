namespace BookingService.API.Entities;

public class Booking
{
    public int Id { get; set; }

    public required string Username { get; set; }

    public int EventId { get; set; }

    public int Amount { get; set; }

}