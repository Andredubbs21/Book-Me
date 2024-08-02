namespace EventService.API.Entities;

public class Event
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public required string Description { get; set; }

    public DateTime Date {get; set;}

    public int MaxCapacity { get; set; }
}