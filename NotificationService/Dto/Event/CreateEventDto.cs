﻿using System.ComponentModel.DataAnnotations;
namespace NotificationService.Dtos.Event;

    public record class CreateEventDto
    (
        [Required][StringLength(50)]string Name,
        [StringLength(250)]string Description,
        [Required]DateTime Date,
        [Required]int MaxCapacity
    );