﻿namespace BookingService.API.Dto;

public record class UserSummaryDto
(
    string Username,
    string LastName,
    string Email
);