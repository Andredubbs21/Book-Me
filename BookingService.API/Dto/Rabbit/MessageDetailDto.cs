namespace BookingService.API.Dto.Rabbit;
 public record messageInfo(
        string email,
        string userName,
        string eventName,
        bool crear
    );