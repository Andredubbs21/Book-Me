﻿namespace EventService.API.Dto;
 public record class EventDetailsDto(
        int Id,
        string Name,
        string Description,
        DateTime Date,
        int MaxCapacity
    );