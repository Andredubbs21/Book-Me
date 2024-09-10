namespace NotificationService.Dto.booking
{
    public class CancelBookingDto
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public int EventId { get; set; }
    }
}
