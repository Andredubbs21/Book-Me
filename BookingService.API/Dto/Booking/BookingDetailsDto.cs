namespace BookingService.API.Dto.booking;


public class BookingDetailsDto
{
    public int Id {get; set;}
    public required string Username {get; set;}
    public int EventId {get; set;}
    public required string EventName {get; set;}
    public int Amount {get; set;}
};