using EventService.API.Entities;
using EventService.API.Dto;
namespace EventService.API.Mapping;
    public static class EventMappingExtensions
    {
        public static Event ToEntity(this CreateEventDto dto) => new Event
        {
            Name = dto.Name,
            Description = dto.Description,
            Date = dto.Date,
            MaxCapacity = dto.MaxCapacity
        };

        public static Event ToEntity(this UpdateEventDto dto, int id) => new Event
        {
            Id = id,
            Name = dto.Name,
            Description = dto.Description,
            Date = dto.Date,
            MaxCapacity = dto.MaxCapacity
        };

        public static EventDetailsDto ToEventDetailsDto(this Event evt) => new EventDetailsDto(
            evt.Id,
            evt.Name,
            evt.Description,
            evt.Date,
            evt.MaxCapacity
        );

        public static EventSummaryDto ToEventSummaryDto(this Event evt) => new EventSummaryDto(
            evt.Id,
            evt.Name,
            evt.Date
        );
    }
