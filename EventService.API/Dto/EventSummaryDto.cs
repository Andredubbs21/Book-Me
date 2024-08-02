namespace EventService.API.Dto;

public record EventSummaryDto(
    int Id,
    string Name,
    DateTime Date
);